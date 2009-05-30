using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace Gablarski.CELT
{
	public class CeltDecoder
		: IDisposable
	{
		private CeltDecoder (IntPtr state, CeltMode mode)
		{
			this.decoderState = state;
			this.Mode = mode;
		}

		public CeltMode Mode
		{
			get;
			private set;
		}

		public unsafe byte[] Decode (byte[] encoded)
		{
			IntPtr pcmptr;
			byte[] pcm = new byte[this.Mode.FrameSize*2];
			fixed (byte* bpcm = pcm)
				pcmptr = new IntPtr((void*)bpcm);

			celt_decode (this.decoderState, encoded, encoded.Length, pcmptr).ThrowIfError();

			return pcm;
		}

		private IntPtr decoderState;

		#region IDisposable Members
		public void Dispose ()
		{
			GC.SuppressFinalize (this);
			Dispose (true);
		}

		private bool disposed;
		protected void Dispose (bool disposing)
		{
			if (this.disposed)
				return;



			this.disposed = true;
		}
		#endregion

		[DllImport ("libcelt.dll", CallingConvention = CallingConvention.Cdecl)]
		private static extern IntPtr celt_decoder_create (IntPtr mode);

		[DllImport ("libcelt.dll", CallingConvention = CallingConvention.Cdecl)]
		private static extern void celt_decoder_destroy (IntPtr decoderState);

		[DllImport ("libcelt.dll", CallingConvention = CallingConvention.Cdecl)]
		private static extern ErrorCode celt_decode (IntPtr decoderState, byte[] data, int length, IntPtr pcm);

		public static CeltDecoder Create (CeltMode mode)
		{
			return new CeltDecoder (celt_decoder_create (mode), mode);
		}
	}
}