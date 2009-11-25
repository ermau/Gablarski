// Copyright (c) 2009, Eric Maupin
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
using System.Text;
using Gablarski.Audio;

namespace Gablarski.OpenAL.Providers
{
	public class OpenALCaptureProvider
		: ICaptureProvider
	{
		#region ICaptureProvider Members
		public int AvailableSampleCount
		{
			get
			{
				if (!this.device.IsOpen)
					return 0;

				return this.device.AvailableSamples;
			}
		}

		public bool CanCaptureStereo
		{
			get { return true; }
		}

		public IAudioDevice Device
		{
			get { return this.device; }
			set
			{
				var cdevice = (value as CaptureDevice);

				if (cdevice == null)
					throw new ArgumentException ("Device must be a OpenAL.CaptureDevice", "value");

				this.device = cdevice;
			}
		}

		public bool IsCapturing
		{
			get;
			private set;
		}

		public void BeginCapture (Audio.AudioFormat format)
		{
			CheckDevice();

			if (!this.device.IsOpen)
				this.device.Open (44100, GetOpenALFormat (format));

			this.IsCapturing = true;
			this.device.StartCapture();
		}

		public void EndCapture ()
		{
			CheckDevice();

			this.IsCapturing = false;
			this.device.StopCapture();
		}

		public byte[] ReadSamples ()
		{
			CheckDevice();

			return this.device.GetSamples();
		}

		public byte[] ReadSamples (int samples)
		{
			CheckDevice();

			return this.device.GetSamples (samples);
		}

		#endregion

		#region IAudioDeviceProvider Members
		
		public IEnumerable<IAudioDevice> GetDevices ()
		{
			return OpenAL.GetCaptureDevices().Cast<IAudioDevice>();
		}

		public IAudioDevice DefaultDevice
		{
			get { return OpenAL.GetDefaultCaptureDevice(); }
		}
		
		#endregion
		
		#region IDisposable Members

		public void Dispose ()
		{
			if (this.device != null)
				this.device.Dispose ();
		}

		#endregion

		private CaptureDevice device;

		private void CheckDevice()
		{
			if (this.device == null)
				throw new InvalidOperationException ("No device set.");
		}

		internal static AudioFormat GetOpenALFormat (Audio.AudioFormat format)
		{
			AudioFormat oalFormat = AudioFormat.Mono16Bit;
			switch (format)
			{
				case Audio.AudioFormat.Mono16Bit:
					oalFormat = AudioFormat.Mono16Bit;
					break;

				case Audio.AudioFormat.Mono8Bit:
					oalFormat = AudioFormat.Mono8Bit;
					break;

				case Audio.AudioFormat.Stereo16Bit:
					oalFormat = AudioFormat.Stereo16Bit;
					break;

				case Audio.AudioFormat.Stereo8Bit:
					oalFormat = AudioFormat.Stereo8Bit;
					break;
			}

			return oalFormat;
		}
	}
}