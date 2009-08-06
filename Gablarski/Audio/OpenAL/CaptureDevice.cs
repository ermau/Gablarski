using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Threading;
using System.Security;
using Gablarski.Client;

namespace Gablarski.OpenAL
{
	[SuppressUnmanagedCodeSecurity]
	public class CaptureDevice
		: Device
	{
		internal CaptureDevice (string deviceName)
			: base (deviceName)
		{
			this.listenerThread = new Thread (this.SampleListener)
			{
				Name = "OpenAL Sample Listener",
				IsBackground = true
			};
		}

		/// <summary>
		/// Gets or sets the minimum samples needed before the <c>SamplesAvailable</c> event is triggered.
		/// </summary>
		public int MinimumSamples
		{
			get { return this.minimumSamples; }
			set { this.minimumSamples = value; }
		}

		/// <summary>
		/// Fired when samples are available for an existing capture.
		/// </summary>
		public event EventHandler<SamplesAvailableEventArgs> SamplesAvailable
		{
			add
			{
				this.StartSampleListener ();

				this.samplesAvailable += value;
			}

			remove
			{
				this.samplesAvailable -= value;

				if (this.samplesAvailable == null)
					this.mre.Reset ();
			}
		}

		/// <summary>
		/// Gets the current format of the capture device.
		/// </summary>
		public AudioFormat Format
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets the current frequency of the capture device.
		/// </summary>
		public uint Frequency
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets the number of available samples.
		/// </summary>
		public int AvailableSamples
		{
			get { return GetSamplesAvailable (); }
		}

		/// <summary>
		/// Opens the capture device with the specified <paramref name="frequency"/> and <paramref name="format"/>.
		/// </summary>
		/// <param name="frequency">The frequency to open the capture device with.</param>
		/// <param name="format">The audio format to open the device with.</param>
		/// <returns>Returns <c>this</c>.</returns>
		public unsafe CaptureDevice Open (uint frequency, AudioFormat format)
		{
			this.Format = format;
			this.Frequency = frequency;

			uint bufferSize = format.GetBytes (format.GetSamplesPerSecond (frequency)) * 2;

			this.Handle = alcCaptureOpenDevice (this.Name, frequency, format, (int)bufferSize);
			OpenAL.ErrorCheck (this);

			pcm = new byte[bufferSize];
			fixed (byte* bppcm = pcm)
				pcmPtr = new IntPtr ((void*)bppcm);

			return this;
		}

		/// <summary>
		/// Starts capturing.
		/// </summary>
		public void StartCapture ()
		{
			this.capturing = true;
			alcCaptureStart (this.Handle);
			OpenAL.ErrorCheck (this);
		}

		/// <summary>
		/// Stops capturing.
		/// </summary>
		public void StopCapture ()
		{
			this.capturing = false;
			alcCaptureStop (this.Handle);
			OpenAL.ErrorCheck (this);
		}

		/// <summary>
		/// Forces the sample listener, if active, to stop.
		/// </summary>
		public void StopSampleListener ()
		{
			this.listening = false;
			this.mre.Set ();
			if (this.listenerThread.IsAlive)
				this.listenerThread.Join ();
		}

		/// <summary>
		/// Gets the available samples.
		/// </summary>
		/// <returns>The available PCM samples.</returns>
		public byte[] GetSamples ()
		{
			return GetSamples (AvailableSamples, false);
		}

		/// <summary>
		/// Gets the available samples and provides the number of samples.
		/// </summary>
		/// <param name="numSamples">The number of samples returned.</param>
		/// <returns>The available PCM samples.</returns>
		public byte[] GetSamples (out int numSamples)
		{
			numSamples = GetSamplesAvailable ();
			return GetSamples (numSamples, false);
		}

		/// <summary>
		/// Gets the specified number of samples.
		/// </summary>
		/// <param name="numSamples">The number of samples to return.</param>
		/// <returns></returns>
		public byte[] GetSamples (int numSamples)
		{
			return GetSamples (numSamples, true);
		}

		protected override void Dispose (bool disposing)
		{
			if (this.Handle == IntPtr.Zero)
				return;

			if (this.IsOpen)
			{
				alcCaptureCloseDevice (this.Handle);
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

		private readonly ManualResetEvent mre = new ManualResetEvent (true);
		private volatile bool capturing;
		private int minimumSamples = 1;
		private volatile bool listening;
		private Thread listenerThread;
		private byte[] pcm;
		private IntPtr pcmPtr;

		protected void StartSampleListener ()
		{
			this.mre.Set ();

			if (this.listening || this.listenerThread.IsAlive)
				return;
			
			lock (this.listenerThread)
			{
				if (!this.listening && !this.listenerThread.IsAlive)
				{
					this.listening = true;
					this.listenerThread.Start ();
				}
			}
		}

		protected virtual void OnCaptureSamplesAvailable (SamplesAvailableEventArgs e)
		{
			var available = this.samplesAvailable;
			if (available != null)
				available (this, e);
		}

		private byte[] GetSamples (int numSamples, bool block)
		{
			byte[] samples = new byte[numSamples * 2];

			while (this.capturing && block && this.AvailableSamples < numSamples)
				Thread.Sleep (1);

			int diff = numSamples - this.AvailableSamples;
			if (diff > 0)
			{
				numSamples -= diff;
				for (int i = diff * 2; i < samples.Length; i++)
				{
					samples[i] = 0;
					samples[++i] = 0;
				}
			}

			alcCaptureSamples (this.Handle, pcmPtr, numSamples);
			OpenAL.ErrorCheck (this);
			Array.Copy (pcm, samples, samples.Length);

			return samples;
		}

		private int GetSamplesAvailable ()
		{
			int samples;
			OpenAL.alcGetIntegerv (this.Handle, ALCEnum.ALC_CAPTURE_SAMPLES, 4, out samples);
			OpenAL.ErrorCheck (this);
			return samples;
		}

		private void SampleListener (object state)
		{
			while (this.listening)
			{
				if (this.IsOpen)
				{
					int numSamples = AvailableSamples;
					if (numSamples > this.minimumSamples)
						OnCaptureSamplesAvailable (new SamplesAvailableEventArgs (numSamples));
				}

				Thread.Sleep (1);
			}
		}
	}
}