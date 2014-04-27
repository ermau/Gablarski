//
// ClientSourceHandler.cs
//
// Author:
//   Eric Maupin <me@ermau.com>
//
// Copyright (c) 2009-2011, Eric Maupin
// Copyright (c) 2011-2014, Xamarin Inc.
// All rights reserved.
//
// Redistribution and use in source and binary forms, with
// or without modification, are permitted provided that
// the following conditions are met:
//
// - Redistributions of source code must retain the above 
//   copyright notice, this list of conditions and the
//   following disclaimer.
//
// - Redistributions in binary form must reproduce the above
//   copyright notice, this list of conditions and the
//   following disclaimer in the documentation and/or other
//   materials provided with the distribution.
//
// - Neither the name of Gablarski nor the names of its
//   contributors may be used to endorse or promote products
//   or services derived from this software without specific
//   prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS
// AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED
// WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
// WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR
// PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT
// HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT,
// INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
// (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE
// GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
// INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY,
// WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
// NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
// THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH
// DAMAGE.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Gablarski.Audio;
using Gablarski.Messages;
using Tempest;

namespace Gablarski.Client
{
	public class ClientSourceHandler
		: IClientSourceHandler
	{
		protected internal ClientSourceHandler (IGablarskiClientContext context)
		{
			if (context == null)
				throw new ArgumentNullException ("context");

			this.context = context;
			this.manager = new ClientSourceManager (context);

			this.context.RegisterMessageHandler<SourceListMessage> (OnSourceListReceivedMessage);
			this.context.RegisterMessageHandler<SourcesRemovedMessage> (OnSourcesRemovedMessage);
			this.context.RegisterMessageHandler<SourceResultMessage> (OnSourceResultMessage);
			this.context.RegisterMessageHandler<ServerAudioDataMessage> (OnServerAudioDataMessage);
			this.context.RegisterMessageHandler<AudioSourceStateChangeMessage> (OnAudioSourceStateChangedMessage);
			this.context.RegisterMessageHandler<SourceMutedMessage> (OnSourceMutedMessage);
		}

		public event EventHandler<ReceivedListEventArgs<AudioSource>> ReceivedSourceList;
		public event EventHandler<ReceivedAudioSourceEventArgs> ReceivedAudioSource;
		public event EventHandler<ReceivedListEventArgs<AudioSource>> AudioSourcesRemoved;
		public event EventHandler<AudioSourceMutedEventArgs> AudioSourceMuted;
		public event EventHandler<AudioSourceEventArgs> AudioSourceStarted;
		public event EventHandler<AudioSourceEventArgs> AudioSourceStopped;
		public event EventHandler<ReceivedAudioEventArgs> ReceivedAudio;

		public int Count
		{
			get { return this.manager.Count; }
		}

		public AudioSource this[int id]
		{
			get { return manager[id]; }
		}

		public IEnumerable<AudioSource> this[IUserInfo user]
		{
			get { return manager[user]; }
		}

		public IEnumerable<AudioSource> Mine
		{
			get { return manager.Where (s => s.OwnerId == context.CurrentUser.UserId); }
		}

		public void Receive (AudioSource source, byte[] audio)
		{
			byte[][] data = new byte[1][];
			data[0] = audio;

			OnReceivedAudio (new ReceivedAudioEventArgs (source, data));
		}

		public void BeginSending (AudioSource source)
		{
			if (source == null)
				throw new ArgumentNullException ("source");
			if (source.OwnerId != this.context.CurrentUser.UserId)
				throw new ArgumentException ("Can not send audio from a source you don't own", "source");

			lock (this.sources) {
				SourceState state;
				if (!this.sources.TryGetValue (source, out state))
					this.sources[source] = state = new SourceState();

				if (state.Codec == null)
					state.Codec = new AudioCodec (source.CodecSettings);

				state.Sequence = 0;
			}

			this.context.Connection.SendAsync (new ClientAudioSourceStateChangeMessage { Starting = true, SourceId = source.Id });

			OnAudioSourceStarted (new AudioSourceEventArgs (source));
		}

		public Task SendAudioDataAsync (AudioSource source, TargetType targetType, int[] targetIds, byte[][] data)
		{
			#if DEBUG
			if (source == null)
				throw new ArgumentNullException ("source");
			if (source.OwnerId != this.context.CurrentUser.UserId)
				throw new ArgumentException ("Can not send audio from a source you don't own", "source");
			if (targetIds == null)
				throw new ArgumentNullException ("targetIds");
			if (data == null)
				throw new ArgumentNullException ("data");
			if (data.Length == 0)
				throw new ArgumentException ("Can not have 0 frames", "data");
			#endif

			byte[][] encoded = new byte[data.Length][];

			int sequence;
			SourceState state;
			lock (this.sources) {
				if (!this.sources.TryGetValue (source, out state))
					throw new InvalidOperationException ("You must call BeginSending on the source before you can call SendAudioData");

				sequence = ++state.Sequence;
			}

			for (int i = 0; i < data.Length; i++)
				encoded[i] = state.Codec.Encode (data[i]);

			return this.context.Connection.SendAsync (new ClientAudioDataMessage
			{
				Sequence = sequence,
				TargetType = targetType,
				TargetIds = targetIds,
				SourceId = source.Id,
				Data = encoded
			});
		}

		public void EndSending (AudioSource source)
		{
			if (source == null)
				throw new ArgumentNullException ("source");
			if (source.OwnerId != this.context.CurrentUser.UserId)
				throw new ArgumentException ("Can not send audio from a source you don't own", "source");

			this.context.Connection.SendAsync (new ClientAudioSourceStateChangeMessage { Starting = false, SourceId = source.Id });

			OnAudioSourceStopped (new AudioSourceEventArgs (source));
		}

		public void ToggleMute (AudioSource source)
		{
			if (source == null)
				throw new ArgumentNullException ("source");

			this.context.Connection.SendAsync (new RequestMuteSourceMessage (source, !source.IsMuted));
		}

		/// <summary>
		/// Requests a source.
		/// </summary>
		/// <param name="targetBitrate">The target bitrate to request.</param>
		/// <param name="name">The user-local name of the source, used to identify the source later.</param>
		/// <remarks>
		/// The server may not agree with the bitrate you request, do not set up audio based on this
		/// target, but on the bitrate of the source you actually receive.
		/// </remarks>
		public void Request (string name, AudioFormat format, short frameSize, int targetBitrate)
		{
			this.context.Connection.SendAsync (new RequestSourceMessage (name, new AudioCodecArgs (format, targetBitrate, frameSize, 10)));
		}

		public AudioSource CreateFake (string name, AudioFormat format, short frameSize)
		{
			if (name == null)
				throw new ArgumentNullException ("name");
			if (format == null)
				throw new ArgumentNullException ("format");
			if (frameSize <= 0)
				throw new ArgumentOutOfRangeException ("frameSize", frameSize, "frameSize must be greater than 0");

			var s = new AudioSource (name, this.nextFakeAudioId--, context.CurrentUser.UserId, format, 0, frameSize, 10);
			manager.Add (s);

			return s;
		}

		public bool GetIsIgnored (AudioSource source)
		{
			return this.manager.GetIsIgnored (source);
		}

		public bool ToggleIgnore (AudioSource source)
		{
			return this.manager.ToggleIgnore (source);
		}

		public void Reset()
		{
			this.sources.Clear();
			this.manager.Clear();
		}

		public IEnumerator<AudioSource> GetEnumerator()
		{
			return manager.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		private int nextFakeAudioId = -1;
		private readonly IGablarskiClientContext context;
		private readonly ClientSourceManager manager;
		private readonly Dictionary<AudioSource, SourceState> sources = new Dictionary<AudioSource, SourceState>();

		class SourceState
		{
			public int Sequence;
			public AudioCodec Codec;
		}

		private void OnSourceMutedMessage (MessageEventArgs<SourceMutedMessage> e)
		{
			var msg = (SourceMutedMessage) e.Message;

			bool fire = false;
			AudioSource s;
			lock (this.manager.SyncRoot)
			{
				s = this.manager[msg.SourceId];
				if (s != null && msg.Unmuted == s.IsMuted)
				{
					this.manager.ToggleMute (s);
					fire = true;
				}
			}

			if (fire)
				OnAudioSourceMuted (new AudioSourceMutedEventArgs (s, msg.Unmuted));
		}

		internal void OnSourceListReceivedMessage (MessageEventArgs<SourceListMessage> e)
		{
			var msg = (SourceListMessage)e.Message;

			this.manager.Update (msg.Sources);

			OnReceivedSourceList (new ReceivedListEventArgs<AudioSource> (msg.Sources));
		}

		internal void OnSourceResultMessage (MessageEventArgs<SourceResultMessage> e)
		{
			var msg = (SourceResultMessage)e.Message;

			AudioSource source = null;
			if (msg.SourceResult == SourceResult.Succeeded || msg.SourceResult == SourceResult.NewSource)
			{
				source = new AudioSource (msg.Source);
				this.manager.Update (source);
			}

			OnReceivedSource (new ReceivedAudioSourceEventArgs (msg.SourceName, source, msg.SourceResult));
		}

		internal void OnSourcesRemovedMessage (MessageEventArgs<SourcesRemovedMessage> e)
		{
			OnSourcesRemoved (new ReceivedListEventArgs<AudioSource> (
				((SourcesRemovedMessage)e.Message).SourceIds
				.Select (id => this.manager[id])
				.Where (s => s != null && this.manager.Remove (s))));
		}

		internal void OnAudioSourceStateChangedMessage (MessageEventArgs<AudioSourceStateChangeMessage> e)
		{
			var msg = (AudioSourceStateChangeMessage)e.Message;

			var source = this.manager[msg.SourceId];

			if (source != null)
			{
				if (msg.Starting)
					OnAudioSourceStarted (new AudioSourceEventArgs (source));
				else
					OnAudioSourceStopped (new AudioSourceEventArgs (source));
			}
		}

		internal void OnServerAudioDataMessage (MessageEventArgs<ServerAudioDataMessage> e)
		{
			var source = this.manager[e.Message.SourceId];
			if (source == null || this.manager.GetIsIgnored (source))
				return;

			int skipped;

			SourceState state;
			lock (this.sources) {
				if (!this.sources.TryGetValue (source, out state)) {
					this.sources[source] = state = new SourceState();
					state.Codec = new AudioCodec (source.CodecSettings);
				}

				skipped = e.Message.Sequence - state.Sequence - 1;
				
				// We can't wait around for the start signal, and the first message
				// in the sequence might be dropped. We'll just assume a new stream
				// if we give a _lower_ sequence than the last one.
				
				if (skipped < 0)
					skipped = e.Message.Sequence - 1;

				state.Sequence = e.Message.Sequence;
			}

			var user = this.context.Users[source.OwnerId];
			if (user == null || this.context.Users.GetIsIgnored (user))
				return;

			int defaultSize = source.CodecSettings.GetBytes (source.CodecSettings.FrameSize);

			byte[][] data = e.Message.Data;
			byte[][] decoded = new byte[data.Length + skipped][];

			for (int i = 0; i < skipped; i++) {
				decoded[i] = state.Codec.Decode (null, defaultSize);
			}

			for (int i = skipped; i < decoded.Length; i++) {
				byte[] frame = data[i - skipped];
				decoded[i] = state.Codec.Decode (frame, frame.Length);
			}

			OnReceivedAudio (new ReceivedAudioEventArgs (source, decoded));
		}

		internal void OnMutedMessage (int sourceId, bool unmuted)
		{
			AudioSource source = this.manager[sourceId];
			if (source == null)
				return;

			this.manager.ToggleIgnore (source);
			OnAudioSourceMuted (new AudioSourceMutedEventArgs (source, unmuted));
		}

		#region Event Invokers

		protected virtual void OnAudioSourceStarted (AudioSourceEventArgs e)
		{
			var started = this.AudioSourceStarted;
			if (started != null)
				started (this, e);
		}

		protected virtual void OnAudioSourceStopped (AudioSourceEventArgs e)
		{
			var stopped = this.AudioSourceStopped;
			if (stopped != null)
				stopped (this, e);
		}

		protected virtual void OnReceivedAudio (ReceivedAudioEventArgs e)
		{
			var received = this.ReceivedAudio;
			if (received != null)
				received (this, e);
		}

		protected virtual void OnReceivedSource (ReceivedAudioSourceEventArgs e)
		{
			var received = this.ReceivedAudioSource;
			if (received != null)
				received (this, e);
		}

		protected virtual void OnReceivedSourceList (ReceivedListEventArgs<AudioSource> e)
		{
			var received = this.ReceivedSourceList;
			if (received != null)
				received (this, e);
		}

		protected virtual void OnSourcesRemoved (ReceivedListEventArgs<AudioSource> e)
		{
			var removed = this.AudioSourcesRemoved;
			if (removed != null)
				removed (this, e);
		}

		protected virtual void OnAudioSourceMuted (AudioSourceMutedEventArgs e)
		{
			var muted = this.AudioSourceMuted;
			if (muted != null)
				muted (this, e);
		}

		#endregion
	}
}
