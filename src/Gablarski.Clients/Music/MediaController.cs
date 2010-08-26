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
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cadenza.Collections;
using Gablarski.Audio;
using Gablarski.Client;

namespace Gablarski.Clients.Media
{
	public class MediaController
		: IMediaController
	{
		public MediaController (IClientContext context, IEnumerable<IMediaPlayer> mediaPlayers)
		{
			this.context = context;
			this.mediaPlayers = mediaPlayers;

			this.context.Audio.SourceFinished += OnSourceFinished;
			this.context.Sources.ReceivedAudio += OnReceivedAudio;
			this.context.Sources.AudioSourceStarted += OnAudioSourceStarted;
			this.context.Sources.AudioSourceStopped += OnAudioSourceStopped;
			
			playerTimer = new Timer (Pulse, null, 0, 2500);
		}

		/// <summary>
		/// Gets or sets the media players to adjust.
		/// </summary>
		public IEnumerable<IMediaPlayer> MediaPlayers
		{
			get { return this.mediaPlayers; }
			set
			{
				SetVolume (VolumeType.Normal);

				lock (this.attachedPlayers)
				{
					this.attachedPlayers.Clear ();
				}

				this.mediaPlayers = value;
			}
		}

		/// <summary>
		/// Gets or sets whether to use the media player's original volume instead of <see cref="VolumeType.Normal"/>.
		/// </summary>
		public bool UseCurrentPlayerVolume
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets whether the user's own speech quiets the music.
		/// </summary>
		public bool UserTalkingCounts
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the volume used when someone is talking.
		/// </summary>
		public int TalkingVolume
		{
			get { return this.talkingVolume; }
			set { this.talkingVolume = value; }
		}

		/// <summary>
		/// Gets or sets the volume used normally.
		/// </summary>
		public int NormalVolume
		{
			get { return this.normalVolume; }
			set { this.normalVolume = value; }
		}

		public void AddTalker ()
		{
			if (Interlocked.Increment (ref this.playing) > 1 || playingSources.Count > 0)
				return;

			SetVolume (VolumeType.Talking);
		}

		public void AddTalker (AudioSource source)
		{
			lock (playingSources)
			{
				if (playingSources.Contains (source))
					return;

				playingSources.Add (source);
			}

			if (this.playing > 0 || playingSources.Count > 1)
				return;

			SetVolume (VolumeType.Talking);
		}

		public void RemoveTalker ()
		{
			if (Interlocked.Decrement (ref this.playing) > 0 || playingSources.Count > 0)
				return;

			SetVolume (VolumeType.Normal);
		}

		public void RemoveTalker (AudioSource source)
		{
			lock (playingSources)
			{
				if (!playingSources.Remove (source))
					return;
			}

			if (this.playing > 0 || playingSources.Count > 0)
				return;

			SetVolume (VolumeType.Normal);
		}

		public void Reset()
		{
			Interlocked.Exchange (ref this.playing, 0);
			
			lock (playingSources)
				playingSources.Clear();

			SetVolume (VolumeType.Normal);
		}

		private int talkingVolume = 30;
		private int normalVolume = 100;
		private IEnumerable<IMediaPlayer> mediaPlayers;

		private int playing;

		private readonly IClientContext context;
		private readonly IAudioReceiver receiver;
		private readonly Dictionary<IMediaPlayer, int> attachedPlayers = new Dictionary<IMediaPlayer, int>();
		private readonly HashSet<AudioSource> playingSources = new HashSet<AudioSource>();
		private readonly Timer playerTimer;

		private enum VolumeType
		{
			Normal,
			Talking
		}

		private void Pulse (object state)
		{
			foreach (var mp in mediaPlayers)
			{
				bool running = mp.IsRunning;

				lock (attachedPlayers)
				{
					bool attached = attachedPlayers.ContainsKey (mp);

					if (!running && attached)
						attachedPlayers.Remove (mp);
					else if (running && !attached)
						attachedPlayers.Add (mp, mp.Volume);
				}
			}
		}

		private void OnAudioSourceStarted (object sender, AudioSourceEventArgs e)
		{
			if (!UserTalkingCounts && e.Source.OwnerId == context.CurrentUser.UserId)
				return;

			AddTalker (e.Source);
		}

		private void OnSourceFinished (object sender, AudioSourceEventArgs e)
		{
			if (!UserTalkingCounts && e.Source.Id > 0 && e.Source.OwnerId == context.CurrentUser.UserId)
				return;

			RemoveTalker (e.Source);
		}

		private void OnAudioSourceStopped (object sender, AudioSourceEventArgs e)
		{
			if (!UserTalkingCounts || !(e.Source.Id > 0 && e.Source.OwnerId == context.CurrentUser.UserId))
				return;

			RemoveTalker (e.Source);
		}

		private void OnReceivedAudio (object sender, ReceivedAudioEventArgs e)
		{
			if (!UserTalkingCounts && e.Source.Id > 0 && e.Source.OwnerId == context.CurrentUser.UserId)
				return;

			AddTalker (e.Source);
		}

		private void SetVolume (VolumeType type)
		{
			Dictionary<IMediaPlayer, int> attached;
			lock (attachedPlayers)
				attached = new Dictionary<IMediaPlayer, int> (attachedPlayers);
			
			foreach (var kvp in attached)
			{
				int volume = (type == VolumeType.Talking) ? TalkingVolume : (!UseCurrentPlayerVolume) ? NormalVolume : kvp.Value;

				try
				{
					kvp.Key.Volume = volume;
				}
				catch
				{
					lock (attachedPlayers)
						attachedPlayers.Remove (kvp.Key);
				}
			}
		}
	}
}