//
// MockAudioCaptureProvider.cs
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
using System.Collections.Generic;
using Gablarski.Audio;

namespace Gablarski.Tests.Mocks.Audio
{
	public class MockAudioCaptureProvider
		: IAudioCaptureProvider
	{
		private int frameSize = 256;
		public int FrameSize
		{
			get { return this.frameSize; }
			set { this.frameSize = value; }
		}

		/// <summary>
		/// Gets a listing of devices for this provider.
		/// </summary>
		/// <returns>A listing of devices to choose from.</returns>
		public IEnumerable<IAudioDevice> GetDevices()
		{
			return new[] { captureDevice };
		}

		/// <summary>
		/// Gets the default device for this provider.
		/// </summary>
		public IAudioDevice DefaultDevice
		{
			get { return captureDevice; }
		}

		public IEnumerable<AudioFormat> SupportedFormats
		{
			get
			{
				return new[]
				{
					AudioFormat.Mono16bitLPCM,
					AudioFormat.Stereo16bitLPCM
				};
			}
		}

		public int AvailableSampleCount
		{
			 get { return this.frameSize; }
		}

		public void Dispose()
		{
		}

		public event EventHandler<SamplesAvailableEventArgs> SamplesAvailable;

		public IAudioDevice Device
		{
			get; set;
		}

		public bool IsCapturing { get; private set; }

		public void Open (AudioFormat format)
		{
		}

		public void Close()
		{
		}

		public void BeginCapture (int frameSize)
		{
			IsCapturing = true;
		}

		public void EndCapture()
		{
			IsCapturing = false;
		}

		public byte[] ReadSamples(int samples)
		{
			return new byte[samples];
		}

		private readonly MockAudioDevice captureDevice = new MockAudioDevice ("MockCaptureDevice");
	}
}