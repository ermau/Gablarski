using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Threading;
using System.Security;

namespace Gablarski.OpenAL
{
	[SuppressUnmanagedCodeSecurity]
	public class CaptureDevice
		: Device
	{
		public CaptureDevice (string deviceName)
			: base (deviceName)
		{
		}

		public int MinimumSamples
		{
			get { return this.minimumSamples; }
			set { this.minimumSamples = value; }
		}

		public event EventHandler<SamplesAvailableEventArgs> SamplesAvailable
		{
			add
			{
				if (this.listenerThread == null)
				{
					this.listening = true;
					(this.listenerThread = new Thread (this.SampleListener)
					{
						Name = "OpenAL Sample Listener",
						IsBackground = true
					}).Start ();
				}

				this.samplesAvailable += value;
			}

			remove
			{
				this.samplesAvailable -= value;

				if (this.samplesAvailable == null)
					this.StopSampleListener ();
			}
		}
		public AudioFormat Format
		{
			get { return this.format; }
		}

		public uint Frequency
		{
			get { return this.frequency; }
		}

		public int AvailableSamples
		{
			get { return GetSamplesAvailable (); }
		}

		public unsafe CaptureDevice Open (uint frequency, AudioFormat format)
		{
			this.format = format;
			this.frequency = frequency;

			this.Handle = alcCaptureOpenDevice (this.DeviceName, frequency, format, (int)format.GetBytesPerSample (frequency) * 2);
			OpenAL.ErrorCheck ();

			pcm = new byte[format.GetBytesPerSample (frequency) * 2];
			fixed (byte* bppcm = pcm)
				pcmPtr = new IntPtr ((void*)bppcm);

			return this;
		}

		public void StartCapture ()
		{
			alcCaptureStart (this.Handle);
			OpenAL.ErrorCheck ();
		}

		public void StopCapture ()
		{
			alcCaptureStop (this.Handle);
			OpenAL.ErrorCheck ();
		}

		public void StopSampleListener ()
		{
			this.listening = false;
			if (this.listenerThread != null)
				this.listenerThread.Join ();
		}

		public byte[] GetSamples ()
		{
			return GetSamples (AvailableSamples);
		}

		public byte[] GetSamples (out int numSamples)
		{
			numSamples = GetSamplesAvailable ();
			return GetSamples (numSamples);
		}

		public byte[] GetSamples (int numSamples)
		{
			byte[] samples = new byte[numSamples * 2];

			alcCaptureSamples (this.Handle, pcmPtr, numSamples);
			OpenAL.ErrorCheck ();
			Array.Copy (pcm, samples, samples.Length);

			return samples;
		}

		protected override void Dispose (bool disposing)
		{
			if (this.Handle == IntPtr.Zero)
				return;

			if (this.IsOpen)
			{
				alcCaptureCloseDevice (this.Handle);
				OpenAL.ErrorCheck ();
				this.Handle = IntPtr.Zero;
			}

			this.disposed = true;
		}

		#region Imports
		[DllImport ("OpenAL32.dll", CallingConvention=CallingConvention.Cdecl)]
		private static extern void alcCaptureStart (IntPtr device);

		[DllImport ("OpenAL32.dll", CallingConvention = CallingConvention.Cdecl)]
		private static extern void alcCaptureStop (IntPtr device);

		[DllImport ("OpenAL32.dll", CallingConvention = CallingConvention.Cdecl)]
		private static extern void alcCaptureSamples (IntPtr device, IntPtr buffer, int numSamples);

		[DllImport ("OpenAL32.dll", CallingConvention = CallingConvention.Cdecl)]
		private static extern IntPtr alcCaptureOpenDevice (string deviceName, uint frequency, AudioFormat format, int bufferSize);

		[DllImport ("OpenAL32.dll", CallingConvention = CallingConvention.Cdecl)]
		private static extern void alcCaptureCloseDevice (IntPtr device);
		#endregion

		private EventHandler<SamplesAvailableEventArgs> samplesAvailable;
		private AudioFormat format;
		private uint frequency;

		private int minimumSamples = 1400;
		private volatile bool listening;
		private Thread listenerThread;
		private byte[] pcm;
		private IntPtr pcmPtr;

		protected virtual void OnCaptureSamplesAvailable (SamplesAvailableEventArgs e)
		{
			var available = this.samplesAvailable;
			if (available != null)
				available (this, e);
		}

		private int GetSamplesAvailable ()
		{
			int samples;
			OpenAL.alcGetIntegerv (this.Handle, ALCEnum.ALC_CAPTURE_SAMPLES, 4, out samples);
			OpenAL.ErrorCheck ();
			return samples;
		}

		private void SampleListener (object state)
		{
			uint sps = this.format.GetSamplesPerSecond (this.frequency);

			while (this.listening)
			{
				int numSamples = AvailableSamples;
				if (numSamples > this.minimumSamples)
				{
					alcCaptureSamples (this.Handle, this.pcmPtr, numSamples);
					OpenAL.ErrorCheck ();

					OnCaptureSamplesAvailable (new SamplesAvailableEventArgs ((byte[])this.pcm.Clone(), numSamples));
				}

				if (numSamples == this.minimumSamples)
					numSamples = 0;

				Thread.Sleep ((int)(((this.minimumSamples - numSamples) / sps) * 1000));
			}
		}
	}

	public class SamplesAvailableEventArgs
		: EventArgs
	{
		public SamplesAvailableEventArgs (byte[] Data, int samples)
		{
			this.Data = Data;
			this.Samples = samples;
		}

		public readonly byte[] Data;
		
		public int Samples
		{
			get;
			private set;
		}
	}
}