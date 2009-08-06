using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gablarski.Audio;

namespace Gablarski.Client
{
	public class VoiceActivation
	{
		public VoiceActivation (ICaptureProvider provider, AudioSource source)
		{
			this.source = source;
			this.capture = provider;
		}
		
		~VoiceActivation ()
		{
			if (this.capture.IsCapturing)
				this.StopListening();
		}

		public event EventHandler<TalkingEventArgs> Talking;

		public void Listen (int vsensitivity)
		{
			this.sensitivity = vsensitivity;
			this.capture.SamplesAvailable += OnSamplesAvailable;
			this.capture.StartCapture();
		}

		public void StopListening()
		{
			this.capture.SamplesAvailable -= OnSamplesAvailable;
			this.capture.EndCapture();
		}

		private int minSamples = 2;
		private int sensitivity;
		private readonly AudioSource source;
		private readonly ICaptureProvider capture;
		
		protected void OnSamplesAvailable (object sender, SamplesAvailableEventArgs e)
		{
			int ms = this.minSamples;

			int nsamples = 0;
			byte[] asamples = this.capture.ReadSamples (this.source.FrameSize);
			for (int i = 0; i < asamples.Length; i += 2)
			{
				short sample = BitConverter.ToInt16 (asamples, i);
				if (sample <= sensitivity)
					continue;

				if (++nsamples == ms)
				{
					OnTalking (new TalkingEventArgs (asamples));
					break;
				}
			}
		}

		protected virtual void OnTalking (TalkingEventArgs e)
		{
			var talking = this.Talking;
			if (talking != null)
				talking (this, e);
		}
	}

	public class TalkingEventArgs
		: EventArgs
	{
		public TalkingEventArgs (byte[] asamples)
		{
			this.Samples = asamples;
		}

		public byte[] Samples { get; private set; }
	}
}