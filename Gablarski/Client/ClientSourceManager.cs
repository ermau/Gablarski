// Copyright (c) 2009, Eric Maupin
// All rights reserved.

// Redistribution and use in source and binary forms, with
// or without modification, are permitted provided that
// the following conditions are met:

// - Redistributions of source code must retain the above 
//   copyright notice, this list of conditions and the
//   following disclaimer.

// - Redistributions in binary form must reproduce the above
//   copyright notice, this list of conditions and the
//   following disclaimer in the documentation and/or other
//   materials provided with the distribution.

// - Neither the name of Gablarski nor the names of its
//   contributors may be used to endorse or promote products
//   derived from this software without specific prior
//   written permission.

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
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Gablarski.Audio;
using Gablarski.Messages;

namespace Gablarski.Client
{
	public class ClientSourceManager
		: IAudioSender, IAudioReceiver, IIndexedEnumerable<int, AudioSource>
	{
		protected internal ClientSourceManager (IClientContext context)
		{
			if (context == null)
				throw new ArgumentNullException ("context");

			this.context = context;
		}

		#region Events
		/// <summary>
		/// A new  or updated source list has been received.
		/// </summary>
		public event EventHandler<ReceivedListEventArgs<AudioSource>> ReceivedSourceList;

		/// <summary>
		/// A new audio source has been received.
		/// </summary>
		public event EventHandler<ReceivedAudioSourceEventArgs> ReceivedAudioSource;

		/// <summary>
		/// An audio source was removed.
		/// </summary>
		public event EventHandler<ReceivedListEventArgs<AudioSource>> AudioSourcesRemoved;

		/// <summary>
		/// An audio source started playing.
		/// </summary>
		public event EventHandler<AudioSourceEventArgs> AudioSourceStarted;

		/// <summary>
		/// An audio source stopped playing.
		/// </summary>
		public event EventHandler<AudioSourceEventArgs> AudioSourceStopped;

		/// <summary>
		/// An audio source has been muted.
		/// </summary>
		public event EventHandler<AudioSourceMutedEventArgs> AudioSourceMuted;

		/// <summary>
		/// Audio has been received.
		/// </summary>
		public event EventHandler<ReceivedAudioEventArgs> ReceivedAudio;
		#endregion

		/// <summary>
		/// Gets a listing of the sources that belong to the current user.
		/// </summary>
		public IEnumerable<AudioSource> Mine
		{
			get
			{
				lock (this.sources)
				{
					return this.Where (s => s.OwnerId == context.CurrentUser.UserId).ToList();
				}
			}
		}

		public AudioSource this[int sourceID]
		{
			get
			{
				AudioSource source;
				lock (this.sources)
				{
					this.sources.TryGetValue (sourceID, out source);
				}
				return source;
			}
		}

		public IEnumerable<AudioSource> this[UserInfo user]
		{
			get
			{
				lock (this.sources)
				{
					return this.sources.Values.Where (s => s.OwnerId == user.UserId).ToList();
				}
			}
		}

		#region Implementation of IEnumerable

		/// <summary>
		/// Returns an enumerator that iterates through the collection.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
		/// </returns>
		/// <filterpriority>1</filterpriority>
		public IEnumerator<AudioSource> GetEnumerator()
		{
			if (this.sources == null)
				yield break;

			lock (this.sources)
			{
				if (this.sources == null)
					yield break;

				foreach (var source in this.sources.Values)
					yield return source;
			}
		}

		/// <summary>
		/// Returns an enumerator that iterates through a collection.
		/// </summary>
		/// <returns>
		/// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
		/// </returns>
		/// <filterpriority>2</filterpriority>
		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}

		#endregion

		/// <summary>
		/// Requests a channel with <paramref name="channels"/> and a default bitrate.
		/// </summary>
		/// <param name="channels">The number of channels to request. 1-2 is the valid range.</param>
		/// <param name="name">The user-local name of the source, used to identify the source later.</param>
		public void Request (string name, int channels)
		{
			Request (name, channels, 0, 0);
		}

		/// <summary>
		/// Requests a channel with <paramref name="channels"/> and a <paramref name="targetBitrate"/>
		/// </summary>
		/// <param name="channels">The number of channels to request. 1-2 is the valid range.</param>
		/// <param name="targetBitrate">The target bitrate to request.</param>
		/// <param name="name">The user-local name of the source, used to identify the source later.</param>
		/// <remarks>
		/// The server may not agree with the bitrate you request, do not set up audio based on this
		/// target, but on the bitrate of the source you actually receive.
		/// </remarks>
		public void Request (string name, int channels, int targetBitrate)
		{
			Request (name, channels, targetBitrate, 0);
		}

		/// <summary>
		/// Requests a channel with <paramref name="channels"/> and a <paramref name="targetBitrate"/>
		/// </summary>
		/// <param name="channels">The number of channels to request. 1-2 is the valid range.</param>
		/// <param name="targetBitrate">The target bitrate to request.</param>
		/// <param name="name">The user-local name of the source, used to identify the source later.</param>
		/// <param name="frameSize">The frame requested frame size. The higher the size, the more latency and more quality.</param>
		/// <remarks>
		/// The server may not agree with the bitrate you request, do not set up audio based on this
		/// target, but on the bitrate of the source you actually receive.
		/// </remarks>
		public void Request (string name, int channels, int targetBitrate, short frameSize)
		{
			this.context.Connection.Send (new RequestSourceMessage (name, channels, targetBitrate, frameSize));
		}

		/// <summary>
		/// Clears the source manager of all sources.
		/// </summary>
		public void Clear()
		{
			lock (this.sources)
			{
				this.sources.Clear();
			}
		}

		/// <summary>
		/// Sends notifications that you're begining to send audio from <paramref name="source"/> to <paramref name="channel"/>.
		/// </summary>
		/// <param name="source">The source to send from.</param>
		/// <param name="channel">The channel to send to.</param>
		/// <exception cref="ArgumentNullException"><paramref name="source"/> is <c>null</c>.</exception>
		/// <exception cref="ArgumentException"><paramref name="source"/> does not belong to you.</exception>
		/// <exception cref="ArgumentNullException"><paramref name="channel"/> is <c>null</c>.</exception>
		public void BeginSending (AudioSource source, ChannelInfo channel)
		{
			if (source == null)
				throw new ArgumentNullException ("source");
			if (source.OwnerId != this.context.CurrentUser.UserId)
				throw new ArgumentException ("Can not send audio from a source you don't own", "source");
			if (channel == null)
				throw new ArgumentNullException ("channel");

			this.context.Connection.Send (new ClientAudioSourceStateChangeMessage (true, source.Id, channel.ChannelId));

			OnAudioSourceStarted (new AudioSourceEventArgs (source));
		}

		/// <summary>
		/// Sends a frame of audio data to the source
		/// </summary>
		/// <param name="source">The source to send from.</param>
		/// <param name="channel">The channel to send to.</param>
		/// <param name="data"></param>
		/// <exception cref="ArgumentNullException"><paramref name="source"/> is <c>null</c>.</exception>
		/// <exception cref="ArgumentException"><paramref name="source"/> does not belong to you.</exception>
		/// <exception cref="ArgumentNullException"><paramref name="channel"/> is <c>null</c>.</exception>
		/// <exception cref="ArgumentNullException"><paramref name="data"/> is <c>null</c>.</exception>
		/// <exception cref="ArgumentException"><paramref name="data"/> is empty.</exception>
		public void SendAudioData (AudioSource source, ChannelInfo channel, byte[] data)
		{
			#if DEBUG
			if (source == null)
				throw new ArgumentNullException ("source");
			if (source.OwnerId != this.context.CurrentUser.UserId)
				throw new ArgumentException ("Can not send audio from a source you don't own", "source");
			if (channel == null)
				throw new ArgumentNullException ("channel");
			if (data == null)
				throw new ArgumentNullException ("data");
			if (data.Length == 0)
				throw new ArgumentException ("Audio frame can not be empty", "data");
			#endif

			this.context.Connection.Send (new SendAudioDataMessage (channel.ChannelId, source.Id, source.Encode (data)));
		}

		/// <summary>
		/// Sends notifications that you're finished sending audio from <paramref name="source"/> to <paramref name="channel"/>.
		/// </summary>
		/// <param name="source">The source to send from.</param>
		/// <param name="channel">The channel to send to.</param>
		/// <exception cref="ArgumentNullException"><paramref name="source"/> is <c>null</c>.</exception>
		/// <exception cref="ArgumentException"><paramref name="source"/> does not belong to you.</exception>
		/// <exception cref="ArgumentNullException"><paramref name="channel"/> is <c>null</c>.</exception>
		public void EndSending (AudioSource source, ChannelInfo channel)
		{
			if (source == null)
				throw new ArgumentNullException ("source");
			if (source.OwnerId != this.context.CurrentUser.UserId)
				throw new ArgumentException ("Can not send audio from a source you don't own", "source");
			if (channel == null)
				throw new ArgumentNullException ("channel");

			this.context.Connection.Send (new ClientAudioSourceStateChangeMessage (false, source.Id, channel.ChannelId));

			OnAudioSourceStopped (new AudioSourceEventArgs (source));
		}

		public void ToggleMute (AudioSource source)
		{
			context.Connection.Send (new RequestMuteMessage { Target = source.Id, Type = MuteType.AudioSource, Unmute = source.IsMuted });
		}

		public bool GetIsIgnored (AudioSource source)
		{
			lock (sources)
			{
				return ignoredSources.Contains (source);
			}
		}

		/// <returns>The new state of ignore on <paramref name="source"/>.</returns>
		public bool ToggleIgnore (AudioSource source)
		{
			lock (sources)
			{
				bool ignored = ignoredSources.Contains (source);

				if (ignored)
					ignoredSources.Remove (source);
				else
					ignoredSources.Add (source);

				return !ignored;
			}
		}

		private readonly IClientContext context;
		private readonly Dictionary<int, AudioSource> sources = new Dictionary<int, AudioSource>();
		private readonly HashSet<AudioSource> ignoredSources = new HashSet<AudioSource>();

		// We'll end up with new instances from the outside world, we can update our
		// own instances no problem.
		internal void UpdateSourceFromExternal (AudioSource updatedSource)
		{
			lock (sources)
			{
				AudioSource source;
				if (!sources.TryGetValue (updatedSource.Id, out source))
				{
					sources[updatedSource.Id] = source = updatedSource;
				}
				else
				{
					CopySource (source, updatedSource);
				}
			}
		}

		internal void UpdateSourcesFromExternal (IEnumerable<AudioSource> updatedSources)
		{
			IEnumerable<AudioSource> updatedAndNew;
			IEnumerable<AudioSource> deleted;

			lock (sources)
			{
				updatedAndNew = updatedSources.Where (s => !this.sources.ContainsValue (s));
				updatedAndNew = updatedAndNew.Concat (this.sources.Values.Intersect (updatedSources)).ToList();
				deleted = this.sources.Values.Where (s => !updatedSources.Contains (s)).ToList();
			}

			foreach (var s in updatedAndNew)
				UpdateSourceFromExternal (s);

			foreach (var d in deleted)
			{
				lock (sources)
				{
					sources.Remove (d.Id);
				}
			}
		}

		private void CopySource (AudioSource target, AudioSource updatedSource)
		{
			target.Name = updatedSource.Name;
			target.Id = updatedSource.Id;
			target.OwnerId = updatedSource.OwnerId;
			target.Bitrate = updatedSource.Bitrate;
			target.IsMuted = updatedSource.IsMuted;

			target.Channels = updatedSource.Channels;
			target.Frequency = updatedSource.Frequency;
			target.FrameSize = updatedSource.FrameSize;
		}

		internal void OnSourceListReceivedMessage (MessageReceivedEventArgs e)
		{
		    var msg = (SourceListMessage)e.Message;

			lock (sources)
			{
				UpdateSourcesFromExternal (msg.Sources);
			}

			OnReceivedSourceList (new ReceivedListEventArgs<AudioSource> (msg.Sources));
		}

		internal void OnSourceResultMessage (MessageReceivedEventArgs e)
		{
		    var msg = (SourceResultMessage)e.Message;

			var source = new AudioSource (msg.Source);

			if (msg.SourceResult == SourceResult.Succeeded || msg.SourceResult == SourceResult.NewSource)
				UpdateSourceFromExternal (source);

		    OnReceivedSource (new ReceivedAudioSourceEventArgs (source, msg.SourceResult));
		}

		internal void OnSourcesRemovedMessage (MessageReceivedEventArgs e)
		{
			var sourceMessage = (SourcesRemovedMessage)e.Message;

			List<AudioSource> removed = new List<AudioSource>();
			lock (sources)
			{
				foreach (int id in sourceMessage.SourceIds)
				{
					if (!this.sources.ContainsKey (id))
						continue;

					removed.Add (this.sources[id]);
					this.sources.Remove (id);
				}
			}

			OnSourcesRemoved (new ReceivedListEventArgs<AudioSource> (removed));
		}

		internal void OnAudioSourceStateChangedMessage (MessageReceivedEventArgs e)
		{
			var msg = (AudioSourceStateChangeMessage)e.Message;

			var source = this[msg.SourceId];

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
			var msg = (AudioDataReceivedMessage)e.Message;

			var source = this[msg.SourceId];
			if (source == null || GetIsIgnored (source))
				return;

			var user = this.context.Users[source.OwnerId];
			if (user != null && !user.IsIgnored)
				OnReceivedAudio (new ReceivedAudioEventArgs (source, msg.Data));
		}

		internal void OnMutedMessage (int sourceId, bool unmuted)
		{
			AudioSource source;
			lock (sources)
			{
				sources.TryGetValue (sourceId, out source);
				if (source == null)
					return;

				source.IsMuted = !unmuted;
			}

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

	#region Event Args
	public class AudioSourceMutedEventArgs
		: AudioSourceEventArgs
	{
		public AudioSourceMutedEventArgs (AudioSource source, bool unmuted)
			: base (source)
		{
			this.Unmuted = unmuted;
		}

		public bool Unmuted { get; set; }
	}

	public class ReceivedAudioSourceEventArgs
		: EventArgs
	{
		public ReceivedAudioSourceEventArgs (AudioSource source, SourceResult result)
		{
			this.Result = result;
			this.Source = source;
		}

		/// <summary>
		/// Gets the result of the source request.
		/// </summary>
		public SourceResult Result
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets the media source of the event.
		/// </summary>
		public AudioSource Source
		{
			get;
			private set;
		}
	}
	#endregion
}