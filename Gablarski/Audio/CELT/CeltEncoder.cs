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
	public class CeltEncoder
		: IDisposable
	{
		private CeltEncoder (CeltMode mode, IntPtr encoderState)
		{
			this.Mode = mode;
			this.encoderState = encoderState;
		}

		/// <summary>
		/// Gets the encoder's current mode.
		/// </summary>
		public CeltMode Mode
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets or sets whether or not to use the long term predictor.
		/// </summary>
		public bool LongTermPredictor
		{
			get { return this.longTermPredictor; }

			set
			{
				this.longTermPredictor = value;
				SetValue (Request.SET_LTP_REQUEST, value ? 1 : 0);
			}
		}

		/// <summary>
		/// Gets or sets the encoder complexity (0-10)
		/// </summary>
		public int Complexity
		{
			get { return this.complexity; }

			set
			{
				if (value < 0 || value > 10)
					throw new ArgumentOutOfRangeException ("value");

				this.complexity = value;
				SetValue (Request.SET_COMPLEXITY_REQUEST, value);
			}
		}

		/// <summary>
		/// Gets the bit-stream version for compatibility checks.
		/// </summary>
		public int BitStreamVersion
		{
			get { return GetValue (Request.GET_BITSTREAM_VERSION); }
		}

		/// <summary>
		/// Gets whether or not the encoder has been disposed.
		/// </summary>
		public bool IsDisposed
		{
			get { return this.disposed; }
		}

		/// <summary>
		/// Encodes PCM to the specified bitrate.
		/// </summary>
		/// <param name="pcm">PCM data.</param>
		/// <param name="bitrate">Target bitrate.</param>
		/// <param name="length">The actual compressed length.</param>
		/// <returns>CELT encoded audio of the supplied PCM.</returns>
		public unsafe byte[] Encode (byte[] pcm, int bitrate, out int length)
		{
			ThrowIfDisposed ();

			int nbCompressedBytes = (bitrate / 8) / (this.Mode.SampleRate / this.Mode.FrameSize);

			IntPtr encodedPtr;
			byte[] encoded = new byte[nbCompressedBytes];
			fixed (byte* benc = encoded)
				encodedPtr = new IntPtr ((void*)benc);
			
			length = celt_encode (this, pcm, IntPtr.Zero, encodedPtr, nbCompressedBytes);

			return encoded;
		}

		//public unsafe byte[] Encode (byte[] pcm, int bitrate, out int length, out short[] synthesis)
		//{
		//    ThrowIfDisposed ();

		//    int nbCompressedBytes = (bitrate / 8) / (this.Mode.SampleRate / this.Mode.FrameSize);

		//    IntPtr encodedPtr;
		//    byte[] encoded = new byte[nbCompressedBytes];
		//    fixed (byte* benc = encoded)
		//        encodedPtr = new IntPtr ((void*)benc);

		//    IntPtr synthPtr;
		//    synthesis = new short[this.Mode.FrameSize * 2];
		//    fixed (short* synth = synthesis)
		//        synthPtr = new IntPtr ((void*)synth);

		//    length = celt_encode (this, pcm, synthPtr, encodedPtr, nbCompressedBytes);

		//    return encoded;
		//}

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

			if (this.encoderState != IntPtr.Zero)
				celt_encoder_destroy (this.encoderState);

			this.encoderState = IntPtr.Zero;

			this.disposed = true;
		}

		~CeltEncoder ()
		{
			Dispose (false);
		}
		#endregion

		private IntPtr encoderState;
		private int complexity = 10;
		private bool longTermPredictor;

		private void ThrowIfDisposed ()
		{
			if (this.IsDisposed)
				throw new ObjectDisposedException (null);
		}

		private int GetValue (Request request)
		{
			ThrowIfDisposed ();

			int ret = 0;
			celt_encoder_ctl (this.encoderState, request, ref ret).ThrowIfError();

			return ret;
		}

		private void SetValue (Request request, int value)
		{
			celt_encoder_ctl (this.encoderState, request, ref value).ThrowIfError ();
		}

		public static implicit operator IntPtr (CeltEncoder encoder)
		{
			return encoder.encoderState;
		}

		/// <summary>
		/// Creates a new <c>CeltEncoder</c> with the specified <paramref name="mode"/>
		/// </summary>
		/// <param name="mode">The mode to use for the encoder.</param>
		/// <returns>A new <c>CeltEncoder</c>.</returns>
		public static CeltEncoder Create (CeltMode mode)
		{
			if (mode == null)
				throw new ArgumentNullException ("mode");

			return new CeltEncoder (mode, celt_encoder_create (mode));
		}

		/// <summary>
		/// Creates a new <c>CeltEncoder</c> with the specified mode settings.
		/// </summary>
		/// <param name="samplingRate">The sampling rate.</param>
		/// <param name="channels">Number of audio channels.</param>
		/// <param name="samplesPerChannel">Number of samples per second per channel.</param>
		/// <returns>A new <c>CeltEncoder</c>.</returns>
		/// <remarks>
		/// This creates a new <see cref="CeltMode"/> before creating the encoder. The mode can
		/// be accessed via the <see cref="CeltEncoder.Mode"/> property.
		/// </remarks>
		public static CeltEncoder Create (int samplingRate, int channels, int samplesPerChannel)
		{
			CeltMode mode = CeltMode.Create (samplingRate, channels, samplesPerChannel);

			return Create (mode);
		}

		[DllImport ("libcelt.dll", CallingConvention = CallingConvention.Cdecl)]
		private static extern IntPtr celt_encoder_create (IntPtr mode);

		[DllImport ("libcelt.dll", CallingConvention = CallingConvention.Cdecl)]
		private static extern void celt_encoder_destroy (IntPtr encoderState);

		[DllImport ("libcelt.dll", CallingConvention = CallingConvention.Cdecl)]
		private static extern int celt_encode (IntPtr encoderState, byte[] pcm, IntPtr optional_synthesis, IntPtr compressed, int nbCompressedBytes);

		[DllImport ("libcelt.dll", CallingConvention = CallingConvention.Cdecl)]
		private static extern ErrorCode celt_encoder_ctl (IntPtr encoderState, Request request, ref int value);
	}
}