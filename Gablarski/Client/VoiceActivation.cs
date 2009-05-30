using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gablarski.Client
{
	public class VoiceActivation
	{
		public VoiceActivation (ICaptureProvider provider)
		{
			this.capture = provider;
			this.capture.SamplesAvailable += OnSamplesAvailable;
		}

		private readonly Queue<byte[]> samples = new Queue<byte[]> ();
		private readonly ICaptureProvider capture;
		
		protected void OnSamplesAvailable (object sender, SamplesAvailableEventArgs e)
		{
			byte[] asamples = this.capture.ReadSamples (e.Samples);

			//lock (samples)
			//{
			//    samples.Enqueue (asamples);
			//}
		}
	}
}