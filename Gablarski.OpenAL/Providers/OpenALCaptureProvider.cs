using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gablarski.Client.Providers.OpenAL
{
	public class OpenALCaptureProvider
		: ICaptureProvider
	{
		#region ICaptureProvider Members

		public event EventHandler<SamplesEventArgs> SamplesAvailable;

		public ICaptureDevice CaptureDevice
		{
			set { throw new NotImplementedException (); }
		}

		public void StartCapture ()
		{
			throw new NotImplementedException ();
		}

		public void EndCapture ()
		{
			throw new NotImplementedException ();
		}

		public bool ReadSamples (out byte[] samples)
		{
			throw new NotImplementedException ();
		}

		public IEnumerable<ICaptureDevice> GetDevices ()
		{
			throw new NotImplementedException ();
		}

		#endregion

		#region IDisposable Members

		public void Dispose ()
		{
			throw new NotImplementedException ();
		}

		#endregion
	}
}