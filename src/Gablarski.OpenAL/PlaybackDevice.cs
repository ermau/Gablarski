// Copyright (c) 2011, Eric Maupin
// All rights reserved.
//
// Redistribution and use in source and binary forms, with
// or without modification, are permitted provided that
// the following conditions are met:
//
// - Redistributions of source code must retain the above 
//   copyright notice, this list of conditions and the
//   following disclaimer.
//
// - Redistributions in binary form must reproduce the above
//   copyright notice, this list of conditions and the
//   following disclaimer in the documentation and/or other
//   materials provided with the distribution.
//
// - Neither the name of Gablarski nor the names of its
//   contributors may be used to endorse or promote products
//   or services derived from this software without specific
//   prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS
// AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED
// WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
// WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR
// PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT
// HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT,
// INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
// (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE
// GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
// INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY,
// WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
// NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
// THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH
// DAMAGE.

using System;
using System.Linq;
using System.Runtime.InteropServices;

namespace Gablarski.OpenAL
{
	public class PlaybackDevice
		: Device
	{
		internal PlaybackDevice (string deviceName)
			: base (deviceName)
		{
		}

		public Context Context
		{
			get;
			internal set;
		}

		/// <summary>
		/// Opens the device.
		/// </summary>
		/// <returns>Returns <c>this</c>.</returns>
		public PlaybackDevice Open ()
		{
			ThrowIfDisposed();

			OpenAL.DebugFormat ("Opening playback device {0}", Name);
			
			this.Handle = alcOpenDevice (this.Name);

			if (this.Handle == IntPtr.Zero)
				throw new Exception ("Device failed to open for an unknown reason.");
			else
				OpenAL.ErrorCheck (this);
			
			return this;
		}

		/// <summary>
		/// Creates and returns a new device context.
		/// </summary>
		/// <returns>The created device context.</returns>
		public Context CreateContext ()
		{
			ThrowIfDisposed();

			return Context.Create (this);
		}

		/// <summary>
		/// Creates, activates and returns a new device context.
		/// </summary>
		/// <returns></returns>
		public Context CreateAndActivateContext ()
		{
			ThrowIfDisposed();

			return Context.CreateAndActivate (this);
		}

		protected override void Dispose (bool disposing)
		{
			if (this.Handle == IntPtr.Zero)
				return;

			if (disposing)
			{
				if (Context != null)
					Context.Dispose();
			}

			OpenAL.DebugFormat ("Destroying playback device {0}", Name);

			alcCloseDevice (this.Handle);
			this.Handle = IntPtr.Zero;
			this.disposed = true;

			OpenAL.DebugFormat ("Destroyed playback device {0}", Name);
		}

		#region Imports
		[DllImport ("OpenAL32.dll", CallingConvention = CallingConvention.Cdecl)]
		private static extern IntPtr alcOpenDevice (string deviceName);

		[DllImport ("OpenAL32.dll", CallingConvention = CallingConvention.Cdecl)]
		private static extern IntPtr alcCloseDevice (IntPtr handle);
		#endregion
	}
}