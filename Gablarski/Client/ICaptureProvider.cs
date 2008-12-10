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
		/// <remarks>
		/// If you consume this event, you MUST handle the bytes. Any samples
		/// provided through this event will be consumed and unavailable from
		/// ReadSamples();
		/// </remarks>
		event EventHandler<SamplesEventArgs> SamplesAvailable;

		ICaptureDevice CaptureDevice { set; }

		void StartCapture ();
		void EndCapture ();

		bool ReadSamples (out byte[] samples);

		IEnumerable<ICaptureDevice> GetDevices ();
	}

	public class SamplesEventArgs
		: EventArgs
	{
		public SamplesEventArgs (byte[] samples)
		{
			this.Samples = samples;
		}

		public byte[] Samples
		{
			get;
			private set;
		}
	}
}