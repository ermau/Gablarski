// Copyright (c) 2010, Eric Maupin
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
using Gablarski.Audio;
using Gablarski.Messages;

namespace Gablarski.Client
{
	public class ClientSourceHandler
		: IClientSourceHandler
	{
		protected internal ClientSourceHandler (IClientContext context, IClientSourceManager manager)
		{
			if (context == null)
				throw new ArgumentNullException ("context");
			if (manager == null)
				throw new ArgumentNullException ("manager");

			this.context = context;
			this.manager = manager;
		}

		public event EventHandler<ReceivedListEventArgs<AudioSource>> ReceivedSourceList;
		public event EventHandler<ReceivedAudioSourceEventArgs> ReceivedAudioSource;
		public event EventHandler<ReceivedListEventArgs<AudioSource>> AudioSourcesRemoved;
		public event EventHandler<AudioSourceMutedEventArgs> AudioSourceMuted;
		public event EventHandler<AudioSourceEventArgs> AudioSourceStarted;
		public event EventHandler<AudioSourceEventArgs> AudioSourceStopped;
		public event EventHandler<ReceivedAudioEventArgs> ReceivedAudio;

		public AudioSource this[int id]
		{
			get { return manager[id]; }
		}

		public IEnumerable<AudioSource> this[UserInfo user]
		{
			get { return manager[user]; }
		}

		public IEnumerable<AudioSource> Mine
		{
			get { return manager.Where (s => s.OwnerId == context.CurrentUser.UserId); }
		}

		public void BeginSending (AudioSource source)
		{
			if (source == null)
				throw new ArgumentNullException ("source");
			if (source.OwnerId != this.context.CurrentUser.UserId)
				throw new ArgumentException ("Can not send audio from a source you don't own", "source");

			lock (sequences)
				sequences[source] = 0;

			this.context.Connection.Send (new ClientAudioSourceStateChangeMessage { Starting = true, SourceId = source.Id });

			OnAudioSourceStarted (new AudioSourceEventArgs (source));
		}

		public void SendAudioData (AudioSource source, TargetType targetType, int[] targetIds, byte[][] data)
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

			for (int i = 0; i < data.Length; i++)
				encoded[i] = source.Encode (data[i]);

			int sequence;
			lock (sequences)
			{
				if (sequences.TryGetValue (source, out sequence))
					sequence++;

				sequences[source] = sequence;
			}

			this.context.Connection.Send (new ClientAudioDataMessage
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

			this.context.Connection.Send (new ClientAudioSourceStateChangeMessage { Starting = false, SourceId = source.Id });

			OnAudioSourceStopped (new AudioSourceEventArgs (source));
		}

		public void ToggleMute (AudioSource source)
		{
			context.Connection.Send (new RequestMuteSourceMessage (source, !source.IsMuted));
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
			this.context.Connection.Send (new RequestSourceMessage (name, new AudioCodecArgs (format, targetBitrate, frameSize, 10)));
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
			this.sequences.Clear();
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

		private readonly IClientContext context;
		private readonly IClientSourceManager manager;
		private readonly Dictionary<AudioSource, int> sequences = new Dictionary<AudioSource, int>();

		internal void OnSourceListReceivedMessage (MessageReceivedEventArgs e)
		{
			var msg = (SourceListMessage)e.Message;

			this.manager.Update (msg.Sources);

			OnReceivedSourceList (new ReceivedListEventArgs<AudioSource> (msg.Sources));
		}

		internal void OnSourceResultMessage (MessageReceivedEventArgs e)
		{
			var msg = (SourceResultMessage)e.Message;

			var source = new AudioSource (msg.Source);

			if (msg.SourceResult == SourceResult.Succeeded || msg.SourceResult == SourceResult.NewSource)
				this.manager.Update (source);

			OnReceivedSource (new ReceivedAudioSourceEventArgs (msg.SourceName, source, msg.SourceResult));
		}

		internal void OnSourcesRemovedMessage (MessageReceivedEventArgs e)
		{
			OnSourcesRemoved (new ReceivedListEventArgs<AudioSource> (
				((SourcesRemovedMessage)e.Message).SourceIds
				.Select (id => this.manager[id])
				.Where (s => s != null && this.manager.Remove (s))));
		}

		internal void OnAudioSourceStateChangedMessage (MessageReceivedEventArgs e)
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

		internal void OnAudioDataReceivedMessage (MessageReceivedEventArgs e)
		{
			var msg = (ServerAudioDataMessage)e.Message;

			var source = this.manager[msg.SourceId];
			if (source == null || this.manager.GetIsIgnored (source))
				return;

			var user = this.context.Users[source.OwnerId];
			if (user != null && !this.context.Users.GetIsIgnored (user))
				OnReceivedAudio (new ReceivedAudioEventArgs (source, msg.Data));
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
