using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gablarski.Audio.Speex;
using Gablarski.Client;

namespace Gablarski.Audio
{
	internal class AudioCaptureEntity
	{
		public AudioCaptureEntity (ICaptureProvider capture, AudioFormat format, ClientAudioSource source, AudioEngineCaptureOptions options)
		{
			this.format = format;
			this.capture = capture;
			this.source = source;
			this.options = options;

			this.frameLength = (this.source.FrameSize/source.Frequency)*1000;

			if (options.Mode == AudioEngineCaptureMode.Activated)
				preprocessor = SpeexPreprocessor.Create (this.source.FrameSize, this.source.Frequency);
		}

		public AudioFormat Format
		{
			get { return format; }
		}

		/// <summary>
		/// Gets the frame length in milliseconds
		/// </summary>
		public int FrameLength
		{
			get { return frameLength; }
		}

		public ICaptureProvider Capture
		{
			get { return this.capture; }
		}

		public ClientAudioSource Source
		{
			get { return this.source; }
		}

		public AudioEngineCaptureOptions Options
		{
			get { return this.options; }
		}

		public SpeexPreprocessor Preprocessor
		{
			get { return this.preprocessor; }
		}

		public ChannelInfo CurrentTargetChannel
		{
			get { return this.channel; }
		}

		public void BeginCapture (ChannelInfo c)
		{
			this.channel = c;
			this.capture.BeginCapture (this.format);
			this.Source.BeginSending (channel);
		}

		public void EndCapture()
		{
			this.capture.EndCapture();
			this.Source.EndSending (this.channel);
		}

		private readonly AudioFormat format;
		private readonly int frameLength;
		private readonly SpeexPreprocessor preprocessor;
		private readonly ICaptureProvider capture;
		private readonly ClientAudioSource source;
		private readonly AudioEngineCaptureOptions options;
		private readonly IPlaybackProvider playback;
		private ChannelInfo channel;

		~AudioCaptureEntity()
		{
			capture.Dispose();
		}
	}
}