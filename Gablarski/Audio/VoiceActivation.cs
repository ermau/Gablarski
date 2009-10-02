using System;

namespace Gablarski.Audio
{
	public class VoiceActivation
	{
		public VoiceActivation (int startVolume, int continueVolume, TimeSpan threshold)
		{
			this.startVol = startVolume;
			this.contVol = continueVolume;
			this.threshold = threshold;
		}

		public bool IsTalking (byte[] samples)
		{
			int total = 0;
			for (int i = 0; i < samples.Length; i += 2)
			{
				total += Math.Abs (BitConverter.ToInt16 (samples, i) - 128);
			}

			int avg = total / (samples.Length / 2);
			DateTime n = DateTime.Now;

			bool result = false;
			if (avg >= ((talking) ? contVol : startVol))
			{
				result = true;
				last = n;
			}
			else if (talking && n.Subtract (last) <= threshold)
			{
				result = true;
			}

			talking = result;

			return result;
		}

		private readonly int startVol;
		private readonly int contVol;
		private readonly TimeSpan threshold;

		private bool talking;
		private DateTime last;
	}
}