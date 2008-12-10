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
		public ICaptureDevice CaptureDevice
		{
			get;
			set;
		}

		public event EventHandler<SamplesEventArgs> SamplesAvailable
		{
			add
			{
				lock (lck)
				{
					if (this.listenerThread == null)
					{
						this.sampleListening = true;
						(this.listenerThread = new Thread (this.SampleListener)
						{
							Name = "Samples Listener",
							IsBackground = true
						}).Start ();
					}

					this.samples += value;
				}
			}

			remove 
			{
				lock (lck)
				{
					this.samples -= value;

					if (this.samples == null)
					{
						this.sampleListening = false;
						this.listenerThread.Join ();
						this.listenerThread = null;
					}
				}
			}
		}
		
		public void StartCapture ()
		{
			Init ();

			ThrowIfNoDevice ();

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

		public unsafe bool ReadSamples (out byte[] samples)
		{
			ThrowIfNoDevice ();

			Init ();

			samples = null;

			int numSamples;
			Alc.alcGetIntegerv (this.captureDevice, Alc.ALC_CAPTURE_SAMPLES, IntPtr.Size, out numSamples);

			if (numSamples <= 0)
				return false;

			lock (lck)
			{
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
			lock (lck)
			{
				this.sampleListening = false;
				if (this.listenerThread != null)
				{
					this.listenerThread.Join ();
					this.listenerThread = null;
				}
			}
		}
		#endregion

		private EventHandler<SamplesEventArgs> samples;

		private bool sampleListening = false;
		private Thread listenerThread;
		private IntPtr captureDevice = IntPtr.Zero;
		private object lck = new object ();
		private byte[] dbuffer;
		private IntPtr buffer;
		private const int BufferSize = 2 * 44100 * 2;

		protected virtual void OnSamplesAvailable (SamplesEventArgs e)
		{
			var se = this.samples;
			if (se != null)
				se (this, e);
		}

		private unsafe void Init ()
		{
			lock (lck)
			{
				if (captureDevice != IntPtr.Zero)
					return;

				this.dbuffer = new byte[BufferSize];
				fixed (byte* pbuffer = dbuffer)
					this.buffer = new IntPtr ((void*)pbuffer);

				captureDevice = Alc.alcCaptureOpenDevice ((string)this.CaptureDevice.Identifier, 44100, Al.AL_FORMAT_MONO16, BufferSize);
			}
		}

		private void SampleListener ()
		{
			while (this.sampleListening)
			{
				lock (lck)
				{
					if (this.captureDevice == IntPtr.Zero)
					{
						Thread.Sleep (1);
						continue;
					}

					int numSamples;
					Alc.alcGetIntegerv (this.captureDevice, Alc.ALC_CAPTURE_SAMPLES, IntPtr.Size, out numSamples);

					if (numSamples > 0)
					{
						byte[] samples;
						this.ReadSamples (out samples);

						this.OnSamplesAvailable (new SamplesEventArgs (samples));
					}
				}

				Thread.Sleep (1);
			}
		}

		protected void ThrowIfNoDevice ()
		{
			if (this.captureDevice == IntPtr.Zero)
				throw new InvalidOperationException ("Capture device not initialized.");
		}
	}
}