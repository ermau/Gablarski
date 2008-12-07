using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Gablarski.Client
{
	public interface ICaptureProvider
		: IDisposable
	{
		ThreadPriority Priority { get; set; }

		void SetDevice (ICaptureDevice device);

		void StartCapture ();
		void EndCapture ();

		bool ReadSamples (out byte[] samples);

		IEnumerable<ICaptureDevice> GetDevices ();
	}
}