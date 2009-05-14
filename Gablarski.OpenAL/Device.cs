using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using Gablarski.Client;

namespace Gablarski.OpenAL
{
	public abstract class Device
		: IDevice
	{
		protected Device (string deviceName)
		{
			this.Name = deviceName;
		}

		/// <summary>
		/// Gets the name of the device
		/// </summary>
		public string Name
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets whether the device is open or not
		/// </summary>
		public bool IsOpen
		{
			get { return (this.Handle != IntPtr.Zero); }
		}

		internal IntPtr Handle;
		protected bool disposed;

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

		protected abstract void Dispose (bool disposing);

		~Device ()
		{
			this.Dispose (false);
		}
		#endregion
	}
}