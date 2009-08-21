using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gablarski.Audio.OpenAL.Providers
{
	public class OpenALCaptureProvider
		: ICaptureProvider
	{
		#region ICaptureProvider Members
		public event EventHandler<SamplesAvailableEventArgs> SamplesAvailable
		{
			add
			{
				this.CheckDevice();
				this.device.SamplesAvailable += value;
			}

			remove
			{
				this.CheckDevice();
				this.device.SamplesAvailable -= value;
			}
		}

		public int AvailableSampleCount
		{
			get { return this.device.AvailableSamples; }
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
			return OpenAL.CaptureDevices.Cast<IAudioDevice>();
		}

		public IAudioDevice DefaultDevice
		{
			get { return OpenAL.DefaultCaptureDevice; }
		}
		
		#endregion
		
		#region IDisposable Members

		public void Dispose ()
		{
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