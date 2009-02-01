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

		public PlaybackDevice Open ()
		{
			this.Handle = alcOpenDevice (this.DeviceName);
			OpenAL.ErrorCheck ();

			return this;
		}

		public Context CreateContext ()
		{
			return Context.Create (this);
		}

		public Context CreateAndActivateContext ()
		{
			return Context.CreateAndActivate (this);
		}

		protected override void Dispose (bool disposing)
		{
			if (this.disposed)
				return;

			if (this.IsOpen)
			{
				alcCloseDevice (this.Handle);
				OpenAL.ErrorCheck ();
				this.Handle = IntPtr.Zero;
			}

			this.disposed = true;
		}

		#region Imports
		[DllImport ("OpenAL32.dll", CallingConvention = CallingConvention.Cdecl)]
		private static extern IntPtr alcOpenDevice (string deviceName);

		[DllImport ("OpenAL32.dll", CallingConvention = CallingConvention.Cdecl)]
		private static extern IntPtr alcCloseDevice (IntPtr handle);
		#endregion
	}
}