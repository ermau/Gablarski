using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Gablarski.Clients.Media;

namespace Gablarski.Winamp
{
	public class WinampIntegration
		: IMediaPlayer
	{
		#region Implementation of IMediaPlayer

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
			get { throw new NotSupportedException(); }
		}

		/// <summary>
		/// Gets the currently playing artist name.
		/// </summary>
		public string ArtistName
		{
			get { throw new NotSupportedException(); }
		}

		/// <summary>
		/// Gets the currently playing album name.
		/// </summary>
		public string AlbumName
		{
			get { throw new NotSupportedException(); }
		}

		/// <summary>
		/// Sets the volume	for the media player. 0 - 100.
		/// </summary>
		/// <exception cref="ArgumentOutOfRangeException">When value is < 0 or > 100.</exception>
		public int Volume
		{
			get { return SendMessage (0x400, -666, 0x7a); }
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