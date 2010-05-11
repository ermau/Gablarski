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
using System.ComponentModel.Composition;
using System.Linq;
using Gablarski.Audio;

namespace Gablarski.OpenAL.Providers
{
	[Export (typeof(IAudioCaptureProvider))]
	public class OpenALCaptureProvider
		: IAudioCaptureProvider
	{
		public OpenALCaptureProvider()
		{
			OpenALRunner.AddUser();
		}

		#region IAudioCaptureProvider Members
		public event EventHandler<SamplesAvailableEventArgs> SamplesAvailable;

		public int AvailableSampleCount
		{
			get { return (!this.device.IsOpen) ? 0 : this.device.AvailableSamples; }
		}

		public IEnumerable<AudioFormat> SupportedFormats
		{
			get
			{
				return new[]
				{
					new AudioFormat (WaveFormatEncoding.LPCM, 1, 8, 44100),
					new AudioFormat (WaveFormatEncoding.LPCM, 1, 16, 44100), 
					new AudioFormat (WaveFormatEncoding.LPCM, 2, 8, 44100), 
					new AudioFormat (WaveFormatEncoding.LPCM, 2, 16, 44100),
				};
			}
		}

		public IAudioDevice Device
		{
			get { return this.device; }
			set
			{
				var cdevice = (value as CaptureDevice);

				if (cdevice == null)
					throw new ArgumentException ("Device must be a non-null OpenAL.CaptureDevice", "value");

				this.device = cdevice;
			}
		}

		public bool IsCapturing
		{
			get;
			private set;
		}

		private bool isOpened;
		public void BeginCapture (AudioFormat format, int captureFrameSize)
		{
			CheckState();

			if (!this.isOpened)
			{
				this.device.Open ((uint)format.SampleRate, format.ToOpenALFormat());
				this.isOpened = true;
			}

			if (!this.device.IsOpen)
				return;

			this.frameSize = captureFrameSize;
			OpenALRunner.AddCaptureProvider (this);
			this.IsCapturing = true;
			this.device.StartCapture();
		}

		private void OnSamplesAvailable (int samplesAvailable)
		{
			var available = this.SamplesAvailable;
			if (available != null)
				available (this, new SamplesAvailableEventArgs (this, samplesAvailable));
		}

		public byte[] ReadSamples (int samples)
		{
			CheckState();

			return this.device.GetSamples (samples);
		}

		public void EndCapture ()
		{
			CheckState();
			OpenALRunner.RemoveUser();

			if (!this.isOpened)
				return;

			if (!this.device.IsOpen)
				return;

			this.IsCapturing = false;
			this.device.StopCapture();
			OpenALRunner.RemoveCaptureProvider (this);
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

		public override string ToString()
		{
			return "OpenAL Capture";
		}
		
		#region IDisposable Members

		public void Dispose ()
		{
			GC.SuppressFinalize (this);
			Dispose (true);
		}

		#endregion

		private CaptureDevice device;
		private bool disposed;
		private int frameSize;

		protected void Dispose (bool disposing)
		{
			if (this.disposed)
				return;

			if (IsCapturing)
				EndCapture();

			if (disposing)
			{
				if (this.device != null)
					this.device.Dispose();
			}

			this.device = null;

			OpenALRunner.RemoveUser();

			this.disposed = true;
		}

		~OpenALCaptureProvider()
		{
			Dispose (false);
		}

		internal void Tick()
		{
			int samples = this.device.AvailableSamples;
			if (samples < this.frameSize)
				OnSamplesAvailable (samples);
		}

		private void CheckState()
		{
			if (this.disposed)
				throw new ObjectDisposedException ("OpenALCaptureProvider");
			if (this.device == null)
				throw new InvalidOperationException ("No device set.");
		}
	}
}