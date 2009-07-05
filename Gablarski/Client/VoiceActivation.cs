using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gablarski.Media.Sources;

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

		public event EventHandler<SamplesAvailableEventArgs> SamplesAvailable;

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

		private int sensitivity;
		private readonly AudioSource source;
		private readonly ICaptureProvider capture;
		
		protected void OnSamplesAvailable (object sender, SamplesAvailableEventArgs e)
		{
			byte[] asamples = this.capture.ReadSamples (this.source.FrameSize);

		}

		protected virtual void OnTalking (SamplesAvailableEventArgs e)
		{
			var talking = this.SamplesAvailable;
			if (talking != null)
				talking (this, e);
		}
	}
}