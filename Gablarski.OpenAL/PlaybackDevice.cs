using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace Gablarski.OpenAL
{
	public class PlaybackDevice
		: Device
	{
		public PlaybackDevice (string deviceName)
			: base (deviceName)
		{
		}

		public void Open ()
		{
			this.Handle = alcOpenDevice (this.DeviceName);
			OpenAL.ErrorCheck ();
			this.IsOpen = true;
		}

		public override void Close ()
		{
			if (this.Handle == IntPtr.Zero)
				return;

			alcCloseDevice (this.Handle);
			OpenAL.ErrorCheck ();
			this.Handle = IntPtr.Zero;
		}

		#region Imports
		[DllImport ("OpenAL32.dll", CallingConvention = CallingConvention.Cdecl)]
		private static extern IntPtr alcOpenDevice (string deviceName);

		[DllImport ("OpenAL32.dll", CallingConvention = CallingConvention.Cdecl)]
		private static extern IntPtr alcCloseDevice (IntPtr handle);
		#endregion
	}
}