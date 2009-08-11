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
		public static SpeexPreprocessor Create (int frameSize, int samplingRate)
		{
			return new SpeexPreprocessor (speex_preprocess_state_init (frameSize, samplingRate));
		}

		internal SpeexPreprocessor (IntPtr state)
		{
			this.state = state;
		}

		public bool VoiceActivityDetection
		{
			get { return (GetValue(SpeexPreprocessorRequest.SPEEX_PREPROCESS_GET_VAD) == 1); }
			set { SetValue (SpeexPreprocessorRequest.SPEEX_PREPROCESS_SET_VAD, (value) ? 1 : 0); }
		}

		public int VoiceActivityStartProbability
		{
			get { return GetValue (SpeexPreprocessorRequest.SPEEX_PREPROCESS_GET_PROB_START); }
			set
			{
#if DEBUG
				if (value < 0 || value > 100)
					throw new ArgumentOutOfRangeException ("value", "value must be 0-100");
#endif

				SetValue (SpeexPreprocessorRequest.SPEEX_PREPROCESS_SET_PROB_START, value);
			}
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

		private int GetValue (SpeexPreprocessorRequest request)
		{
			int value = 0;
			if (speex_preprocess_ctl (this.state, request, ref value) == CtlResult.Error)
				throw new Exception("GetValue " + request);

			return value;
		}

		private void SetValue (SpeexPreprocessorRequest request, int value)
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
		private static extern int speex_preprocess_run (IntPtr state, ref short[] frames);

		[DllImport ("libspeexdsp.dll")]
		private static extern CtlResult speex_preprocess_ctl (IntPtr state, SpeexPreprocessorRequest request, ref int data);
		
		private enum CtlResult
		{
			Success = 0,
			Error = -1
		}

		private enum SpeexPreprocessorRequest
		{
			SPEEX_PREPROCESS_SET_DENOISE = 0,
			SPEEX_PREPROCESS_GET_DENOISE = 1,
			SPEEX_PREPROCESS_SET_AGC = 2,
			SPEEX_PREPROCESS_GET_AGC = 3,
			SPEEX_PREPROCESS_SET_VAD = 4,
			SPEEX_PREPROCESS_GET_VAD = 5,

			SPEEX_PREPROCESS_SET_PROB_START = 14,
			SPEEX_PREPROCESS_GET_PROB_START = 15,
			SPEEX_PREPROCESS_SET_PROB_CONTINUE = 16,
			SPEEX_PREPROCESS_GET_PROB_CONTINUE = 17,
		}
		// ReSharper restore InconsistentNaming
	}
}