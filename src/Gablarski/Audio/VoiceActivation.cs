// Copyright (c) 2011, Eric Maupin
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
		public VoiceActivation (AudioSource source, int startVolume, int continueVolume, TimeSpan threshold)
		{
			if (source == null)
				throw new ArgumentNullException ("source");
			if (source.Channels != 1 || source.WaveEncoding != WaveFormatEncoding.LPCM)
				throw new ArgumentException ("Can not perform voice activation on a non-mono or non-LPCM source.");

			this.source = source;
			this.length = (double) this.source.FrameSize / source.SampleRate;

			this.startVol = startVolume;
			this.contVol = continueVolume;
			this.threshold = threshold.TotalSeconds;
		}

		public int GetLevel (byte[] samples)
		{
			int avg;
			switch (this.source.BitsPerSample) {
				case 16: {
					int total = 0;
					for (int i = 0; i < samples.Length; i += 2) {
						#if !SAFE
						unsafe {
							fixed (byte* numRef = &(samples[i]))
								total += Math.Abs (*((short*) numRef) - 128);
						}
						#else
						total += Math.Abs (BitConverter.ToInt16 (samples, i) - 128);
						#endif
					}

					avg = total / (samples.Length / 2);
					break;
				}
					
				case 8: {
					int total = 0;
					for (int i = 0; i < samples.Length; ++i)
						total += Math.Abs (samples[i] - 128);

					avg = total / samples.Length;
					break;
				}
					
				default:
					throw new NotSupportedException ("BitsPerSample " + this.source.BitsPerSample + " is unsupported for VoiceActivation");
			}

			return avg;
		}

		public bool IsTalking (byte[] samples)
		{
			int avg = GetLevel (samples);
			this.time += this.length;

			bool result = false;
			if (avg >= ((this.talking) ? contVol : startVol)) {
				result = true;
				this.time = 0;
			} else if (this.talking && this.time <= threshold) {
				result = true;
			}

			this.talking = result;

			return result;
		}

		private readonly AudioSource source;
		private readonly int startVol;
		private readonly int contVol;
		private readonly double threshold;
		private readonly double length;

		private bool talking;
		private double time;
	}
}