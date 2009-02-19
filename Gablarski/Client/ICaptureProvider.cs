using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gablarski.Client
{
	public interface ICaptureProvider
		: IDisposable
	{
		object Device { get; set; }

		void StartCapture ();
		void EndCapture ();

		byte[] ReadSamples ();

		IEnumerable<object> GetDevices ();
	}
}