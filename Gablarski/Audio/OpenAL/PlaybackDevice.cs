using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace Gablarski.Audio.OpenAL
{
	public class PlaybackDevice
		: Device
	{
		internal PlaybackDevice (string deviceName)
			: base (deviceName)
		{
		}

		/// <summary>
		/// Opens the device.
		/// </summary>
		/// <returns>Returns <c>this</c>.</returns>
		public PlaybackDevice Open ()
		{
			this.Handle = alcOpenDevice (this.Name);
			OpenAL.ErrorCheck (this);

			if (this.Handle == IntPtr.Zero)
				throw new Exception ("Device failed to open for an unknown reason.");

			return this;
		}

		/// <summary>
		/// Creates and returns a new device context.
		/// </summary>
		/// <returns>The created device context.</returns>
		public Context CreateContext ()
		{
			return Context.Create (this);
		}

		/// <summary>
		/// Creats, activates and returns a new device context.
		/// </summary>
		/// <returns></returns>
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