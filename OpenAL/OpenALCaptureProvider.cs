using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tao.OpenAl;
using System.Threading;

namespace Gablarski.Client.Providers.OpenAL
{
	public class OpenALCaptureProvider
		: ICaptureProvider
	{
		#region ICaptureProvider Members

		public ThreadPriority Priority
		{
			get { return this.priority; }
			set { this.priority = value; }
		}

		public void SetDevice (ICaptureDevice device)
		{
			captureDevice = Alc.alcCaptureOpenDevice ((string)device.Identifier, 44100, Al.AL_FORMAT_MONO16, 44100 * 2);
		}

		public void StartCapture ()
		{
			ThrowIfNoDevice ();

			Init ();

			lock (lck)
			{
				Alc.alcCaptureStart (this.captureDevice);
			}
		}

		public void EndCapture ()
		{
			ThrowIfNoDevice ();

			lock (lck)
			{
				Alc.alcCaptureStop (this.captureDevice);
			}
		}

		public bool ReadSamples (out byte[] samples)
		{
			lock (lck)
			{
				samples = null;

				int numSamples;
				Alc.alcGetIntegerv (this.captureDevice, Alc.ALC_CAPTURE_SAMPLES, IntPtr.Size, out numSamples);

				if (numSamples <= 0)
					return false;

				samples = new byte[numSamples * 2];
				Alc.alcCaptureSamples (this.captureDevice, this.buffer, numSamples);
				Array.Copy (this.dbuffer, samples, samples.Length);

				return true;
			}
		}

		public IEnumerable<ICaptureDevice> GetDevices ()
		{
			return new List<ICaptureDevice> { new OpenALCaptureDevice (Alc.alcGetString (IntPtr.Zero, Alc.ALC_CAPTURE_DEFAULT_DEVICE_SPECIFIER)) };
		}

		public void Dispose ()
		{
			if (this.captureThread != null)
				this.captureThread.Join ();
		}
		#endregion

		private bool capturing = false;
		private ThreadPriority priority = ThreadPriority.Normal;
		private Thread captureThread;
		private IntPtr captureDevice = IntPtr.Zero;
		private object lck = new object ();

		private int bufferSize = (2 * 44100) * 2;
		private byte[] dbuffer;
		private IntPtr buffer;

		private unsafe void Init ()
		{
			lock (lck)
			{
				if (this.captureThread != null)
					return;

				(this.captureThread = new Thread (this.Capture)
				{
					Name = "OpenAL Capture",
					Priority = this.Priority,
					IsBackground = true
				}).Start ();

				this.dbuffer = new byte[this.bufferSize];
				fixed (byte* pbuffer = dbuffer)
					this.buffer = new IntPtr ((void*)pbuffer);
			}
		}

		private void Capture ()
		{
			while (true)
			{

			}
		}

		protected void ThrowIfNoDevice ()
		{
			if (this.captureDevice == IntPtr.Zero)
				throw new InvalidOperationException ("Capture device not initialized.");
		}
	}
}