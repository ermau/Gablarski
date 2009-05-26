using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gablarski.Client;

namespace Gablarski.OpenAL.Providers
{
	public class CaptureProvider
		: ICaptureProvider
	{
		#region ICaptureProvider Members
		public event EventHandler<SamplesAvailableEventArgs> SamplesAvailable
		{
			add { this.device.SamplesAvailable += value; }
			remove { this.device.SamplesAvailable -= value; }
		}

		public IDevice Device
		{
			get { return this.device; }
			set
			{
				var cdevice = (value as CaptureDevice);

				if (cdevice == null)
					throw new ArgumentException ("Device must be a OpenAL.CaptureDevice", "value");

				this.device = cdevice;
				this.device.Open (44100, AudioFormat.Mono16Bit);
			}
		}

		public void StartCapture ()
		{
			this.device.StartCapture();
		}

		public void EndCapture ()
		{
			this.device.StopCapture();
		}

		public byte[] ReadSamples ()
		{
			return this.device.GetSamples();
		}

		#endregion

		#region IDeviceProvider Members
		
		public IEnumerable<IDevice> GetDevices ()
		{
			return OpenAL.CaptureDevices.Cast<IDevice>();
		}

		public IDevice DefaultDevice
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
	}
}