using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gablarski.Client
{
	public interface ICaptureProvider
		: IDisposable
	{
		IDevice Device { get; set; }

		void StartCapture ();
		void EndCapture ();

		byte[] ReadSamples ();

		IEnumerable<IDevice> GetDevices ();
	}
}