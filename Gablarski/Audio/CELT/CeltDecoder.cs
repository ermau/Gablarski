// Copyright (c) 2009, Eric Maupin
// All rights reserved.

// Redistribution and use in source and binary forms, with
// or without modification, are permitted provided that
// the following conditions are met:

// - Redistributions of source code must retain the above 
//   copyright notice, this list of conditions and the
//   following disclaimer.

// - Redistributions in binary form must reproduce the above
//   copyright notice, this list of conditions and the
//   following disclaimer in the documentation and/or other
//   materials provided with the distribution.

// - Neither the name of Gablarski nor the names of its
//   contributors may be used to endorse or promote products
//   derived from this software without specific prior
//   written permission.

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

		/// <summary>
		/// Gets the current mode for this decoder.
		/// </summary>
		public CeltMode Mode
		{
			get;
			private set;
		}

		/// <summary>
		/// Decodes CELT compressed data to PCM.
		/// </summary>
		/// <param name="encoded">The CELT encoded data.</param>
		/// <returns>The CELT decoded PCM.</returns>
		public unsafe byte[] Decode (byte[] encoded)
		{
			#if DEBUG
			if (encoded == null)
				throw new ArgumentNullException ("encoded");
			#endif

			IntPtr pcmptr;
			byte[] pcm = new byte[this.Mode.FrameSize*2];
			fixed (byte* bpcm = pcm)
				pcmptr = new IntPtr((void*)bpcm);

			celt_decode (this.decoderState, encoded, encoded.Length, pcmptr).ThrowIfError();

			return pcm;
		}

		private readonly IntPtr decoderState;

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

			celt_decoder_destroy (this.decoderState);

			this.disposed = true;
		}
		#endregion

		#region Imports
		// ReSharper disable InconsistentNaming
		[DllImport ("libcelt.dll", CallingConvention = CallingConvention.Cdecl)]
		private static extern IntPtr celt_decoder_create (IntPtr mode);

		[DllImport ("libcelt.dll", CallingConvention = CallingConvention.Cdecl)]
		private static extern void celt_decoder_destroy (IntPtr decoderState);

		[DllImport ("libcelt.dll", CallingConvention = CallingConvention.Cdecl)]
		private static extern ErrorCode celt_decode (IntPtr decoderState, byte[] data, int length, IntPtr pcm);
		// ReSharper restore InconsistentNaming
		#endregion

		/// <summary>
		/// Creates a new <c>CeltDecoder</c> with the specified <paramref name="mode"/>.
		/// </summary>
		/// <param name="mode">The mode to create the decoder with.</param>
		/// <returns>A new <c>CeltDecoder</c>.</returns>
		public static CeltDecoder Create (CeltMode mode)
		{
			return new CeltDecoder (celt_decoder_create (mode), mode);
		}
	}
}