﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace Gablarski.CELT
{
	public class CeltMode
		: IDisposable
	{
		private CeltMode (IntPtr mode, int sampleRate)
		{
			this.mode = mode;
			this.SampleRate = sampleRate;
		}

		public bool IsDisposed
		{
			get { return this.disposed; }
		}

		/// <summary>
		/// Gets the sample rate for this mode.
		/// </summary>
		public int SampleRate
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets the number of channels used in this mode.
		/// </summary>
		public int NumberOfChannels
		{
			get { return GetValue (Request.GET_NB_CHANNELS); }
		}

		/// <summary>
		/// Gets the lookahead used in this mode.
		/// </summary>
		public int LookAhead
		{
			get { return GetValue (Request.GET_LOOKAHEAD); }
		}

		/// <summary>
		/// Gets the frame size used in this mode.
		/// </summary>
		public int FrameSize
		{
			get { return GetValue (Request.GET_FRAME_SIZE); }
		}

		private IntPtr mode;

		private int GetValue (Request request)
		{
			if (this.IsDisposed)
				throw new ObjectDisposedException (null);

			int ret = 0;
			celt_mode_info (this.mode, request, ref ret).ThrowIfError ();

			return ret;
		}

		private void SetValue (Request request, int value)
		{
			celt_mode_info (this.mode, request, ref value).ThrowIfError ();
		}

		#region IDisposable Members
		public void Dispose ()
		{
			GC.SuppressFinalize (this);
			this.Dispose (true);
		}

		private bool disposed;
		protected void Dispose (bool disposing)
		{
			if (this.disposed)
				return;

			if (this.mode != IntPtr.Zero)
				celt_mode_destroy (this.mode);

			this.mode = IntPtr.Zero;

			this.disposed = true;
		}

		~CeltMode ()
		{
			Dispose (false);
		}
		#endregion

		public static implicit operator IntPtr (CeltMode cmode)
		{
			return cmode.mode;
		}

		/// <summary>
		/// Creates a new mode to be passed to an encoder or decoder.
		/// </summary>
		/// <param name="samplingRate">Sampling rate (32000 to 96000 Hz).</param>
		/// <param name="channels">Number of channels.</param>
		/// <param name="frameSize">Number of samples (per channel) to encode in each packet (even values; 64-512).</param>
		/// <returns>A newly created mode.</returns>
		public static CeltMode Create (int samplingRate, int channels, int frameSize)
		{
			if (samplingRate < 32000 || samplingRate > 96000)
				throw new ArgumentOutOfRangeException ("samplingRate");

			if (frameSize < 64 || frameSize > 512)
				throw new ArgumentOutOfRangeException ("samplesPerChannel");

			if (frameSize % 2 != 0)
				throw new ArgumentException ("You must have an even number of samples per channel", "samplesPerChannel");

			IntPtr error;
			IntPtr mode = celt_mode_create (samplingRate, channels, frameSize, out error);

			((ErrorCode)error).ThrowIfError ();
			return new CeltMode (mode, samplingRate);
		}

		[DllImport ("libcelt.dll", CallingConvention= CallingConvention.Cdecl)]
		private static extern IntPtr celt_mode_create (int Fs, int channels, int frame_size, out IntPtr error);

		[DllImport ("libcelt.dll", CallingConvention = CallingConvention.Cdecl)]
		private static extern void celt_mode_destroy (IntPtr mode);

		[DllImport ("libcelt.dll", CallingConvention = CallingConvention.Cdecl)]
		private static extern ErrorCode celt_mode_info (IntPtr mode, Request request, ref int value);
	}
}