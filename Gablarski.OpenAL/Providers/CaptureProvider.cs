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

		public IDevice Device
		{
			get
			{
				throw new NotImplementedException ();
			}
			set
			{
				throw new NotImplementedException ();
			}
		}

		public void StartCapture ()
		{
			throw new NotImplementedException ();
		}

		public void EndCapture ()
		{
			throw new NotImplementedException ();
		}

		public byte[] ReadSamples ()
		{
			throw new NotImplementedException ();
		}

		public IEnumerable<IDevice> GetDevices ()
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