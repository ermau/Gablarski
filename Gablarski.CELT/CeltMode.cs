using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace Gablarski.CELT
{
	public class CeltMode
		: IDisposable
	{
		private CeltMode (IntPtr mode)
		{
			this.mode = mode;
		}

		public bool IsDisposed
		{
			get { return this.disposed; }
		}

		private IntPtr mode;

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
		/// <param name="samplesPerChannel">Number of samples (per channel) to encode in each packet (even values; 65-512).</param>
		/// <returns>A newly created mode.</returns>
		public static CeltMode Create (int samplingRate, int channels, int samplesPerChannel)
		{
			if (samplingRate < 32000 || samplingRate > 96000)
				throw new ArgumentOutOfRangeException ("samplingRate");

			if (samplesPerChannel < 65 || samplesPerChannel > 512)
				throw new ArgumentOutOfRangeException ("samplesPerChannel");

			if (samplesPerChannel % 2 != 0)
				throw new ArgumentException ("You must have an even number of samples per channel", "samplesPerChannel");

			IntPtr error;
			IntPtr mode = celt_mode_create (samplingRate, channels, samplesPerChannel, out error);

			((ErrorCode)error).ThrowIfError ();
			return new CeltMode (mode);
		}

		[DllImport ("libcelt.dll")]
		private static extern IntPtr celt_mode_create (int Fs, [MarshalAs (UnmanagedType.SysInt)] int channels, [MarshalAs (UnmanagedType.SysInt)] int frame_size, out IntPtr error);

		[DllImport ("libcelt.dll")]
		private static extern void celt_mode_destroy (IntPtr mode);

		[DllImport ("libcelt.dll")]
		[return: MarshalAs (UnmanagedType.SysInt)]
		private static extern int celt_mode_info (IntPtr mode, [MarshalAs (UnmanagedType.SysInt)] Request request, out IntPtr value);
	}
}