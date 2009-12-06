// Copyright (c) 2009, Eric Maupin
// All rights reserved.
//
// Redistribution and use in source and binary forms, with
// or without modification, are permitted provided that
// the following conditions are met:
//
// - Redistributions of source code must retain the above 
//   copyright notice, this list of conditions and the
//   following disclaimer.
//
// - Redistributions in binary form must reproduce the above
//   copyright notice, this list of conditions and the
//   following disclaimer in the documentation and/or other
//   materials provided with the distribution.
//
// - Neither the name of Gablarski nor the names of its
//   contributors may be used to endorse or promote products
//   or services derived from this software without specific
//   prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS
// AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED
// WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
// WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR
// PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT
// HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT,
// INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
// (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE
// GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
// INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY,
// WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
// NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
// THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH
// DAMAGE.

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
				//total += Math.Abs ((samples[i] | (samples[i + 1] << 8)) - 128);
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