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
				preprocessor = new SpeexPreprocessor (this.source.FrameSize, this.source.Frequency);
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
			this.Source.EndSending ();
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