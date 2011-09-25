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
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Gablarski.Audio.Speex
{
	public class SpeexPreprocessor
		: IDisposable
	{
		public SpeexPreprocessor (int frameSize, int samplingRate)
		{
			this.state = speex_preprocess_state_init (frameSize, samplingRate);
		}

		internal SpeexPreprocessor (IntPtr state)
		{
			this.state = state;
		}

		#region Noise
		public bool Denoise
		{
			get { return GetValue (SPEEX_PREPROCESS.GET_DENOISE) == 1; }
			set { SetValue (SPEEX_PREPROCESS.SET_DENOISE, value); }
		}

		/// <summary>
		/// Gets or sets maximum attenuation of the noise in dB
		/// </summary>
		public int NoiseSupress
		{
			get { return GetValue (SPEEX_PREPROCESS.GET_NOISE_SUPPRESS); }
			set { SetValue (SPEEX_PREPROCESS.SET_NOISE_SUPPRESS, value); }
		}
		#endregion

		#region AGC
		public bool AutomaticGainControl
		{
			get { return GetValue (SPEEX_PREPROCESS.GET_AGC) == 1; }
			set { SetValue (SPEEX_PREPROCESS.SET_AGC, value); }
		}

		/// <summary>
		/// Gets or sets the maximal gain increase in dB/second
		/// </summary>
		public int AutomaticGainControlIncrement
		{
			get { return GetValue (SPEEX_PREPROCESS.GET_AGC_INCREMENT); }
			set { SetValue (SPEEX_PREPROCESS.SET_AGC_INCREMENT, value); }
		}

		/// <summary>
		/// Gets or sets the maximal gain decrease in dB/second
		/// </summary>
		public int AutomaticGainControlDecrement
		{
			get { return GetValue (SPEEX_PREPROCESS.GET_AGC_DECREMENT); }
			set { SetValue (SPEEX_PREPROCESS.SET_AGC_DECREMENT, value); }
		}

		/// <summary>
		/// Gets or sets the maximal gain in dB
		/// </summary>
		public int AutomaticGainControlMaximum
		{
			get { return GetValue (SPEEX_PREPROCESS.GET_AGC_MAX_GAIN); }
			set { SetValue (SPEEX_PREPROCESS.SET_AGC_MAX_GAIN, value); }
		}

		/// <summary>
		/// Gets loudness
		/// </summary>
		public int AutomaticGainControlLoudness
		{
			get { return GetValue (SPEEX_PREPROCESS.GET_AGC_LOUDNESS); }
		}
		#endregion

		#region Dereverb
		public bool Dereverb
		{
			get { return GetValue (SPEEX_PREPROCESS.GET_DEREVERB) == 1; }
			set { SetValue (SPEEX_PREPROCESS.GET_DEREVERB, value); }
		}

		public int DereverbLevel
		{
			get { return GetValue (SPEEX_PREPROCESS.GET_DEREVERB_LEVEL); }
			set { SetValue (SPEEX_PREPROCESS.SET_DEREVERB_LEVEL, value); }
		}

		public int DereverbDecay
		{
			get { return GetValue (SPEEX_PREPROCESS.GET_DEREVERB_DECAY); }
			set { SetValue (SPEEX_PREPROCESS.SET_DEREVERB_DECAY, value); }
		}
		#endregion

		#region VAD
		public bool VoiceActivityDetection
		{
			get { return (GetValue(SPEEX_PREPROCESS.GET_VAD) == 1); }
			set { SetValue (SPEEX_PREPROCESS.SET_VAD, value); }
		}

		public int VoiceActivityProbability
		{
			get { return GetValue (SPEEX_PREPROCESS.GET_PROB); }
		}

		/// <summary>
		/// Gets or sets the probability required for the VAD to go from silence to voice (0-100)
		/// </summary>
		public int VoiceActivityStartProbability
		{
			get { return GetValue (SPEEX_PREPROCESS.GET_PROB_START); }
			set
			{
				#if DEBUG
				if (value < 0 || value > 100)
				    throw new ArgumentOutOfRangeException ("value", "value must be 0-100");
				#endif

				SetValue (SPEEX_PREPROCESS.SET_PROB_START, value);
			}
		}

		/// <summary>
		/// Gets or sets the probability required for the VAD to stay in the voice state (0-100)
		/// </summary>
		public int VoiceActivityContinueProbability
		{
			get { return GetValue (SPEEX_PREPROCESS.GET_PROB_CONTINUE); }

			set
			{
				#if DEBUG
				if (value < 0 || value > 100)
				    throw new ArgumentOutOfRangeException ("value", "value must be 0-100");
				#endif

				SetValue (SPEEX_PREPROCESS.SET_PROB_CONTINUE, value);
			}
		}
		#endregion

		public bool Preprocess (byte[] pcm)
		{
			return speex_preprocess_run (this.state, pcm) == 1;
		}

		#region IDisposable Members
		public void Dispose ()
		{
			GC.SuppressFinalize (this);
			Dispose (true);
		}

		private bool disposed;
		protected virtual void Dispose (bool disposing)
		{
			if (this.disposed)
				return;

			if (this.state != IntPtr.Zero)
				speex_preprocess_state_destroy (this.state);

			this.state = IntPtr.Zero;

			this.disposed = true;
		}

		~SpeexPreprocessor ()
		{
			Dispose (false);
		}
		#endregion

		private IntPtr state;

		private int GetValue (SPEEX_PREPROCESS request)
		{
			int value = 0;
			if (speex_preprocess_ctl (this.state, request, ref value) == CtlResult.Error)
				throw new Exception("GetValue " + request);

			return value;
		}

		private void SetValue (SPEEX_PREPROCESS request, bool value)
		{
			SetValue (request, (value) ? 1 : 0);
		}

		private void SetValue (SPEEX_PREPROCESS request, int value)
		{
			if (speex_preprocess_ctl (this.state, request, ref value) == CtlResult.Error)
				throw new Exception ("SetValue " + request);
		}

		// ReSharper disable InconsistentNaming
		[DllImport ("libspeexdsp.dll")]
		private static extern IntPtr speex_preprocess_state_init (int frame_size, int sampling_rate);

		[DllImport ("libspeexdsp.dll")]
		private static extern void speex_preprocess_state_destroy (IntPtr state);

		[DllImport ("libspeexdsp.dll")]
		private static extern int speex_preprocess_run (IntPtr state, byte[] frames);

		[DllImport ("libspeexdsp.dll")]
		private static extern CtlResult speex_preprocess_ctl (IntPtr state, SPEEX_PREPROCESS request, ref int data);
		
		private enum CtlResult
		{
			Success = 0,
			Error = -1
		}

		private enum SPEEX_PREPROCESS
		{
			SET_DENOISE = 0,
			GET_DENOISE = 1,
			
			SET_AGC = 2,
			GET_AGC = 3,
			SET_AGC_LEVEL = 6,
			GET_AGC_LEVEL = 7,
			SET_AGC_INCREMENT = 26,
			GET_AGC_INCREMENT = 27,
			SET_AGC_DECREMENT = 28,
			GET_AGC_DECREMENT = 29,
			SET_AGC_MAX_GAIN = 30,
			GET_AGC_MAX_GAIN = 31,
			GET_AGC_LOUDNESS = 33,
			
			SET_DEREVERB = 8,
			GET_DEREVERB = 9,
			SET_DEREVERB_LEVEL = 10,
			GET_DEREVERB_LEVEL = 11,
			SET_DEREVERB_DECAY = 12,
			GET_DEREVERB_DECAY = 13,
			
			SET_VAD = 4,
			GET_VAD = 5,
			GET_PROB = 45,
			SET_PROB_START = 14,
			GET_PROB_START = 15,
			SET_PROB_CONTINUE = 16,
			GET_PROB_CONTINUE = 17,

			SET_NOISE_SUPPRESS = 18,
			GET_NOISE_SUPPRESS = 19,
			
			SET_ECHO_SUPPRESS = 20,
			GET_ECHO_SUPPRESS = 21,
			SET_ECHO_SUPPRESS_ACTIVE = 22,
			GET_ECHO_SUPPRESS_ACTIVE = 23,
			SET_ECHO_STATE = 24,
			GET_ECHO_STATE = 25,
		}
		// ReSharper restore InconsistentNaming
	}
}