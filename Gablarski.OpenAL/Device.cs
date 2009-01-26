using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace Gablarski.OpenAL
{
	public class Device
	{
		public Device (string deviceName)
		{
			IntPtr handle = alcOpenDevice (deviceName);
			OpenAL.ErrorCheck ();
		}

		internal Device (IntPtr handle)
		{
			this.Handle = handle;
			this.IsOpen = true;
		}

		public bool IsOpen
		{
			get;
			private set;
		}

		public void Open ()
		{
		}

		internal readonly IntPtr Handle;

		[DllImport ("OpenAL32.dll")]
		private static extern IntPtr alcOpenDevice (string deviceName);
	}
}