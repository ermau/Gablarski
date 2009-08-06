﻿using System;
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

		public IDevice Device
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

		public void StartCapture ()
		{
			CheckDevice();

			if (!this.device.IsOpen)
				this.device.Open (44100, AudioFormat.Mono16Bit);

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

		private void CheckDevice()
		{
			if (this.device == null)
				throw new InvalidOperationException ("No device set.");
		}
	}
}