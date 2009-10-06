using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Gablarski.Audio;
using Gablarski.Client;

namespace Gablarski.Clients.Music
{
	public class MediaPlayerIntegration
		: IDisposable
	{
		public MediaPlayerIntegration (ClientSourceManager sourceManager, IEnumerable<IMediaPlayer> mediaPlayers)
		{
			this.mediaPlayers = mediaPlayers;

			this.sources = sourceManager;
			this.sources.AudioSourceStarted += OnAudioSourceStarted;
			this.sources.AudioSourceStopped += OnAudioSourceStopped;

			playerTimer = new Timer (Pulse, null, 0, 2500);
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

		private int talkingVolume = 30;
		private int normalVolume = 100;

		private int playing;
		private readonly ClientSourceManager sources;
		private readonly IEnumerable<IMediaPlayer> mediaPlayers;
		private readonly HashSet<IMediaPlayer> attachedPlayers = new HashSet<IMediaPlayer>();
		private readonly Timer playerTimer;

		private void Pulse (object state)
		{
			foreach (var mp in mediaPlayers)
			{
				bool running = mp.IsRunning;

				lock (attachedPlayers)
				{
					if (!running && attachedPlayers.Contains (mp))
						attachedPlayers.Remove (mp);
					else if (running && !attachedPlayers.Contains (mp))
						attachedPlayers.Add (mp);
				}
			}
		}

		private void OnAudioSourceStopped (object sender, AudioSourceEventArgs e)
		{
			if (Interlocked.Decrement (ref this.playing) > 0)
				return;

			SetVolume (NormalVolume);
		}

		private void OnAudioSourceStarted (object sender, AudioSourceEventArgs e)
		{
			if (Interlocked.Increment (ref this.playing) > 1)
				return;

			SetVolume (TalkingVolume);
		}

		private void SetVolume (int volume)
		{
			IEnumerable<IMediaPlayer> attached; 
			lock (attachedPlayers)
			{
				attached = attachedPlayers.ToList();
			}
			
			foreach (var mp in attached)
			{
				try
				{
					mp.Volume = volume;
				}
				catch
				{
					lock (attachedPlayers)
					{
						attachedPlayers.Remove (mp);
					}
				}
			}
		}

		#region Implementation of IDisposable

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		/// <filterpriority>2</filterpriority>
		public void Dispose()
		{
			Dispose (true);
			GC.SuppressFinalize (this);
		}

		protected void Dispose (bool disposing)
		{
			this.playerTimer.Dispose();
		}

		#endregion
	}
}