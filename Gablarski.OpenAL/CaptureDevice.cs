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

		#region Imports
		[DllImport ("OpenAL32.dll", CallingConvention=CallingConvention.Cdecl)]
		private static extern void alcCaptureStart (IntPtr device);

		[DllImport ("OpenAL32.dll", CallingConvention = CallingConvention.Cdecl)]
		private static extern void alcCaptureStop (IntPtr device);

		[DllImport ("OpenAL32.dll", CallingConvention = CallingConvention.Cdecl)]
		private static extern void alcCaptureSamples (IntPtr device, IntPtr buffer, int numSamples);
		#endregion

		private volatile bool listening;
		private Thread listenerThread;

		protected virtual void OnSamplesAvailable (SamplesAvailableEventArgs e)
		{
			this.SamplesAvailable (this, e);
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
			int minimumSamples = (int)state;

			while (this.listening)
			{

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