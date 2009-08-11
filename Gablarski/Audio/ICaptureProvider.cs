using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gablarski.Audio
{
	public interface ICaptureProvider
		: IAudioDeviceProvider, IDisposable
	{
		IAudioDevice Device { get; set; }
		bool IsCapturing { get; }

		event EventHandler<SamplesAvailableEventArgs> SamplesAvailable;

		void StartCapture ();
		void EndCapture ();

		byte[] ReadSamples ();
		byte[] ReadSamples (int samples);
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