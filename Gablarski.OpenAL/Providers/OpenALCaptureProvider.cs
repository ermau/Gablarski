using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gablarski.OpenAL;

namespace Gablarski.Client.Providers
{
	public class OpenALCaptureProvider
		: ICaptureProvider
	{
		public OpenALCaptureProvider ()
		{
			this.capture = OpenAL.OpenAL.DefaultCaptureDevice.Open (44100, AudioFormat.Mono16Bit);
		}

		#region ICaptureProvider Members

		public event EventHandler<SamplesEventArgs> SamplesAvailable
		{
			add
			{
				this.capture.SamplesAvailable += capture_SamplesAvailable;
				samples += value;
			}

			remove
			{
				this.capture.SamplesAvailable -= capture_SamplesAvailable;
				samples -= value;
			}
		}

		void capture_SamplesAvailable (object sender, SamplesAvailableEventArgs e)
		{
			samples (sender, new SamplesEventArgs (e.Data));
		}

		public ICaptureDevice CaptureDevice
		{
			set {  }
		}

		public void StartCapture ()
		{
			this.capture.StartCapture ();
		}

		public void EndCapture ()
		{
			this.capture.StopCapture ();
		}

		public bool ReadSamples (out byte[] samples)
		{
			if (this.capture.AvailableSamples <= 0)
			{
				samples = null;
				return false;
			}

			samples = this.capture.GetSamples ();
			return true;
		}

		public IEnumerable<ICaptureDevice> GetDevices ()
		{
			return new ICaptureDevice[] { null };
		}

		#endregion

		#region IDisposable Members

		public void Dispose ()
		{
			throw new NotImplementedException ();
		}

		#endregion

		private readonly CaptureDevice capture;
		private EventHandler<SamplesEventArgs> samples;
	}
}