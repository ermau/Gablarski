//
// OpenALCaptureProvider.cs
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
using System.Threading;
using Gablarski.Audio;

namespace Gablarski.OpenAL.Providers
{
	public class OpenALCaptureProvider
		: IAudioCaptureProvider
	{
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
					new AudioFormat (WaveFormatEncoding.LPCM, 2, 16, 44100)
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

				if (this.isOpened) {
					Close();
					this.device = cdevice;
					Open (this.format);
				} else
					this.device = cdevice;
			}
		}

		public bool IsCapturing
		{
			get;
			private set;
		}

		public void Open (AudioFormat openingFormat)
		{
			if (openingFormat == null)
				throw new ArgumentNullException ("openingFormat");
			if (this.isOpened && !Equals (openingFormat, this.format))
				throw new ArgumentException ("Provider is already open with a different format");

			var d = this.device;
			if (d == null)
				throw new InvalidOperationException ("Device is not set");

			this.isOpened = true;
			d.Open ((uint) openingFormat.SampleRate, openingFormat.ToOpenALFormat());
			this.format = openingFormat;
		}

		public void Close()
		{
			EndCapture();
			this.isOpened = false;
			this.device = null;
		}

		public void BeginCapture (int captureFrameSize)
		{
			CheckState();
			OpenALRunner.AddUser();

			this.frameSize = captureFrameSize;
			OpenALRunner.AddCaptureProvider (this);
			IsCapturing = true;
			this.device.StartCapture();
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

			IsCapturing = false;
			OpenALRunner.RemoveCaptureProvider (this);

			if (this.device.IsOpen)
				this.device.StopCapture();
		}

		public IEnumerable<IAudioDevice> GetDevices ()
		{
			return OpenAL.GetCaptureDevices();
		}

		public IAudioDevice DefaultDevice
		{
			get { return OpenAL.GetDefaultCaptureDevice(); }
		}

		public void Dispose ()
		{
			GC.SuppressFinalize (this);
			Dispose (true);
		}

		private bool isOpened;
		private CaptureDevice device;
		private bool disposed;
		private int frameSize;
		private AudioFormat format;

		protected void Dispose (bool disposing)
		{
			if (this.disposed)
				return;

			OpenAL.DebugFormat ("Freeing OpenALCaptureProvider. Disposing: {0}", disposing);

			if (disposing)
			{
				if (IsCapturing)
					EndCapture();

				if (this.device != null)
					this.device.Dispose();
			}

			this.device = null;
			this.disposed = true;
		}

		~OpenALCaptureProvider()
		{
			Dispose (false);
		}

		internal void Tick()
		{
			if (!this.device.IsConnected)
			{
				var d = DefaultDevice;
				if (d == null || d.Equals (this.device))
					return;

				this.device.Dispose();

				Device = d;
				this.device.Open ((uint) this.format.SampleRate, this.format.ToOpenALFormat());
				this.device.StartCapture();
			}

			int samples = this.device.AvailableSamples;
			if (samples > this.frameSize)
				OnSamplesAvailable (samples);
		}

		private void CheckState()
		{
			if (this.disposed)
				throw new ObjectDisposedException ("OpenALCaptureProvider");
			if (this.device == null)
				throw new InvalidOperationException ("No device set.");
		}

		private void OnSamplesAvailable (int samplesAvailable)
		{
			var available = SamplesAvailable;
			if (available != null)
				available (this, new SamplesAvailableEventArgs (this, samplesAvailable));
		}
	}
}