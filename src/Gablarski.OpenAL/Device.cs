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
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using Gablarski.Audio;

namespace Gablarski.OpenAL
{
	public abstract class Device
		: IAudioDevice
	{
		protected Device (string deviceName)
		{
			Name = deviceName;
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

		/// <summary>
		/// Gets the refresh rate of the device.
		/// </summary>
		public int Refresh
		{
			get
			{
				ThrowIfDisposed();

				int refresh;
				OpenAL.alcGetIntegerv (this.Handle, ALCEnum.ALC_REFRESH, 1, out refresh);
				return refresh;
			}
		}

		public override string ToString ()
		{
			return Name;
		}

		public override bool Equals (object obj)
		{
			if (ReferenceEquals (null, obj))
				return false;
			if (ReferenceEquals (this, obj))
				return true;
			if (obj.GetType() != GetType())
				return false;
			return Equals ((Device) obj);
		}

		protected bool Equals (Device other)
		{
			return string.Equals (Name, other.Name);
		}

		public override int GetHashCode()
		{
			return Name.GetHashCode();
		}

		public static bool operator == (Device left, Device right)
		{
			return Equals (left, right);
		}

		public static bool operator != (Device left, Device right)
		{
			return !Equals (left, right);
		}

		internal IntPtr Handle;
		protected bool disposed;

		public void Dispose ()
		{
			Dispose (true);
			GC.SuppressFinalize (this);
		}

		protected abstract void Dispose (bool disposing);

		~Device ()
		{
			Dispose (false);
		}

		[Conditional ("DEBUG")]
		protected void ThrowIfDisposed()
		{
			if (this.disposed)
				throw new ObjectDisposedException ("Device");
		}
	}
}