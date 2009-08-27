using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gablarski.Audio.Speex;

namespace Gablarski.Audio
{
	internal class AudioPlaybackEntity
	{
		public AudioPlaybackEntity (IPlaybackProvider playback, AudioSource source, AudioEnginePlaybackOptions options)
		{
			this.playback = playback;
			this.source = source;
			this.options = options;

			buffer = new SpeexJitterBuffer (source.FrameSize);
			this.frameTimeSpan = TimeSpan.FromSeconds ((double)source.FrameSize/source.Frequency);
		}

		public DateTime Last
		{
			get; set;
		}

		public IPlaybackProvider Playback
		{
			get { return this.playback; }
		}

		public AudioSource Source
		{
			get { return this.source; }
		}

		public AudioEnginePlaybackOptions Options
		{
			get { return this.options; }
		}

		public TimeSpan FrameTimeSpan
		{
			get { return frameTimeSpan; }
		}

		public SpeexJitterBuffer Buffer
		{
			get { return this.buffer; }
		}

		public bool Playing
		{
			get; set;
		}
		
		private readonly TimeSpan frameTimeSpan;
		private readonly IPlaybackProvider playback;
		private readonly AudioSource source;
		private readonly AudioEnginePlaybackOptions options;
		private readonly SpeexJitterBuffer buffer;
	}
}