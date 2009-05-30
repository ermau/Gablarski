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
		private CeltEncoder (IntPtr encoderState)
		{
			this.encoderState = encoderState;
		}

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
		/// Gets the number of channels used in the current mode.
		/// </summary>
		public int NumberOfChannels
		{
			get { return GetValue (Request.GET_NB_CHANNELS); }
		}

		/// <summary>
		/// Gets the lookahead used in the current mode.
		/// </summary>
		public int LookAhead
		{
			get { return GetValue (Request.GET_LOOKAHEAD); }
		}

		/// <summary>
		/// Gets the frame size used in the current mode.
		/// </summary>
		public int FrameSize
		{
			get { return GetValue (Request.GET_LOOKAHEAD); }
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

		public byte[] Encode (short[] pcm)
		{
			ThrowIfDisposed ();

			int maxCompressedBytes;
			byte[] encoded;
			int compressedBytes = celt_encode (this, pcm, null, out encoded, out maxCompressedBytes);

			return encoded;
		}

		public byte[] Encode (short[] pcm, out short[] compressed)
		{
			ThrowIfDisposed ();

			int maxCompressedBytes;
			byte[] encoded;
			int compressedBytes = celt_encode (this, pcm, out compressed, out encoded, out maxCompressedBytes);

			return encoded;
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
			if (this.IsDisposed)
				throw new ObjectDisposedException (null);

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

		public static CeltEncoder Create (CeltMode mode)
		{
			if (mode == null)
				throw new ArgumentNullException ("mode");

			return new CeltEncoder (celt_encoder_create (mode));
		}

		public static CeltEncoder Create (int samplingRate, int channels, int samplesPerChannel)
		{
			CeltMode mode = CeltMode.Create (samplingRate, channels, samplesPerChannel);

			return Create (mode);
		}

		[DllImport ("libcelt.dll")]
		private static extern IntPtr celt_encoder_create (IntPtr mode);

		[DllImport ("libcelt.dll")]
		private static extern void celt_encoder_destroy (IntPtr encoderState);

		[DllImport ("libcelt.dll")]
		[return: MarshalAs (UnmanagedType.SysInt)]
		private static extern int celt_encode (IntPtr encoderState, short[] pcm, out short[] optional_synthesis, out byte[] compressed, [MarshalAs (UnmanagedType.SysInt)] out int maxCompressedBytes);

		[DllImport ("libcelt.dll")]
		[return: MarshalAs (UnmanagedType.SysInt)]
		private static extern int celt_encode (IntPtr encoderState, short[] pcm, short[] optional_synthesis, out byte[] compressed, [MarshalAs (UnmanagedType.SysInt)] out int maxCompressedBytes);

		[DllImport ("libcelt.dll")]
		[return: MarshalAs (UnmanagedType.SysInt)]
		private static extern ErrorCode celt_encoder_ctl (IntPtr encoderState, [MarshalAs (UnmanagedType.SysInt)] Request request, [MarshalAs (UnmanagedType.SysInt)] ref int value);
	}
}