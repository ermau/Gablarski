using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace Gablarski.OpenAL
{
	public class Device
		: IDisposable
	{
		public Device (string deviceName)
		{
			this.DeviceName = deviceName;
		}

		public string DeviceName
		{
			get;
			private set;
		}

		public bool IsOpen
		{
			get;
			private set;
		}

		public void Open ()
		{
			this.Handle = alcOpenDevice (this.DeviceName);
			OpenAL.ErrorCheck ();
			this.IsOpen = true;
		}

		public void Close ()
		{
			if (this.Handle == IntPtr.Zero)
				return;
			
			alcCloseDevice (this.Handle);
			OpenAL.ErrorCheck ();
			this.Handle = IntPtr.Zero;
		}

		internal IntPtr Handle;
		private bool disposed;

		#region Imports
		[DllImport ("OpenAL32.dll", CallingConvention = CallingConvention.Cdecl)]
		private static extern IntPtr alcOpenDevice (string deviceName);

		[DllImport ("OpenAL32.dll", CallingConvention = CallingConvention.Cdecl)]
		private static extern IntPtr alcCloseDevice (IntPtr handle);
		#endregion

		#region IDisposable Members

		public void Dispose ()
		{
			Dispose (true);
			GC.SuppressFinalize (this);
		}

		protected void Dispose (bool disposing)
		{
			this.Close ();
			this.disposed = true;
		}

		~Device ()
		{
			this.Dispose (false);
		}
		#endregion
	}
}