using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gablarski.Client
{
	public interface ICaptureProvider
		: IDeviceProvider, IDisposable
	{
		IDevice Device { get; set; }

		event EventHandler<SamplesAvailableEventArgs> SamplesAvailable;

		void StartCapture ();
		void EndCapture ();

		byte[] ReadSamples ();
	}

	public class SamplesAvailableEventArgs
		: EventArgs
	{
		public SamplesAvailableEventArgs (int samples)
		{
			this.Samples = samples;
		}

		public int Samples
		{
			get;
			private set;
		}
	}
}