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
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Gablarski.Clients.Media;

namespace Gablarski.Winamp
{
	[Export (typeof(IMediaPlayer))]
	public class WinampIntegration
		: IMediaPlayer
	{
		#region Implementation of IMediaPlayer

		public string Name
		{
			get { return "WinAmp"; }
		}

		/// <summary>
		/// Gets whether or not the media player is currently running.
		/// </summary>
		public bool IsRunning
		{
			get
			{
				FindWinamp();

				return (this.handle != IntPtr.Zero);
			}
		}

		/// <summary>
		/// Gets the currently playing song name.
		/// </summary>
		public string SongName
		{
			get { return "Not supported."; }
		}

		/// <summary>
		/// Gets the currently playing artist name.
		/// </summary>
		public string ArtistName
		{
			get { return "Not supported."; }
		}

		/// <summary>
		/// Gets the currently playing album name.
		/// </summary>
		public string AlbumName
		{
			get { return "Not supported."; }
		}

		/// <summary>
		/// Sets the volume	for the media player. 0 - 100.
		/// </summary>
		/// <exception cref="ArgumentOutOfRangeException">When value is < 0 or > 100.</exception>
		public int Volume
		{
			get { return (int)Math.Round (SendMessage (0x400, -666, 0x7a) / 2.55); }
			set { SendMessage (0x400, (int) Math.Round (value * 2.55, 0), 0x7a); }
		}

		#endregion

		private IntPtr handle;

		private void FindWinamp()
		{
			this.handle = IntPtr.Zero;

			var p = Process.GetProcessesByName ("winamp").FirstOrDefault();
			if (p != null)
			{
				this.handle = p.MainWindowHandle;
			}
		}

		private int SendMessage (int wMsg, int wParam, int lParam)
		{
			if (this.handle == IntPtr.Zero)
				return 0;

			return SendMessageA (this.handle, wMsg, wParam, lParam);
		}

		[DllImport ("user32.dll", CharSet = CharSet.Auto)]
		private static extern int SendMessageA (IntPtr hwnd, int msg, int wParam, int lParam);
	}
}