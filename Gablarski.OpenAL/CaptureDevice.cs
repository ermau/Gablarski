using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Threading;

namespace Gablarski.OpenAL
{
	public class CaptureDevice
		: Device
	{
		public CaptureDevice (string deviceName)
			: base (deviceName)
		{
		}

		public event EventHandler<SamplesAvailableEventArgs> SamplesAvailable = delegate { };

		public AudioFormat Format
		{
			get { return this.format; }
		}

		public uint Frequency
		{
			get { return this.frequency; }
		}

		public unsafe void Open (uint frequency, AudioFormat format)
		{
			this.format = format;
			this.frequency = frequency;

			this.IsOpen = true;
			alcCaptureOpenDevice (this.DeviceName, frequency, format, (int)format.GetBytesPerSample (frequency) * 2);
			OpenAL.ErrorCheck ();

			pcm = new byte[format.GetBytesPerSample (frequency) * 2];
			fixed (byte* bppcm = pcm)
				pcmPtr = new IntPtr ((void*)bppcm);
		}

		public override void Close ()
		{
			alcCaptureCloseDevice (this.Handle);
			OpenAL.ErrorCheck ();
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

		public void StartSampleListener (int minimumSamples)
		{
			(this.listenerThread = new Thread (this.SampleListener)
			{
				Name = "OpenAL Sample Listener",
				IsBackground = true
			}).Start (minimumSamples);
		}

		public void StopSampleListener ()
		{
			this.listening = false;
			if (this.listenerThread != null)
				this.listenerThread.Join ();
		}

		public int GetSamplesAvailable ()
		{
			int samples;
			OpenAL.alcGetIntegerv (this.Handle, ALCEnum.ALC_CAPTURE_SAMPLES, 4, out samples);
			OpenAL.ErrorCheck ();
			return samples;
		}

		#region Imports
		[DllImport ("OpenAL32.dll", CallingConvention=CallingConvention.Cdecl)]
		private static extern void alcCaptureStart (IntPtr device);

		[DllImport ("OpenAL32.dll", CallingConvention = CallingConvention.Cdecl)]
		private static extern void alcCaptureStop (IntPtr device);

		[DllImport ("OpenAL32.dll", CallingConvention = CallingConvention.Cdecl)]
		private static extern void alcCaptureSamples (IntPtr device, IntPtr buffer, int numSamples);

		[DllImport ("OpenAL32.dll", CallingConvention = CallingConvention.Cdecl)]
		private static extern void alcCaptureOpenDevice (string deviceName, uint frequency, AudioFormat format, int bufferSize);

		[DllImport ("OpenAL32.dll", CallingConvention = CallingConvention.Cdecl)]
		private static extern void alcCaptureCloseDevice (IntPtr device);
		#endregion

		private AudioFormat format;
		private uint frequency;

		private volatile bool listening;
		private Thread listenerThread;
		private byte[] pcm;
		private IntPtr pcmPtr;

		protected virtual void OnSamplesAvailable (SamplesAvailableEventArgs e)
		{
			this.SamplesAvailable (this, e);
		}

		private void SampleListener (object state)
		{
			int minimumSamples = (int)state;

			uint sps = this.format.GetSamplesPerSecond (this.frequency);

			while (this.listening)
			{
				int samples = GetSamplesAvailable ();
				if (samples > minimumSamples)
				{
					alcCaptureSamples (this.Handle, this.pcmPtr, GetSamplesAvailable ());

					OnSamplesAvailable (new SamplesAvailableEventArgs (this.pcm));
				}

				if (samples == minimumSamples)
					samples = 0;

				Thread.Sleep ((int)(((minimumSamples - samples) / sps) * 1000));
			}
		}
	}

	public class SamplesAvailableEventArgs
		: EventArgs
	{
		public SamplesAvailableEventArgs (byte[] Data)
		{
			this.Data = Data;
		}

		public readonly byte[] Data;
	}
}