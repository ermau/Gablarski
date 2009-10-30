using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Gablarski.Clients;
using Gablarski.Clients.Media;
using iTunesLib;

namespace Gablarski.iTunes
{
	public class iTunesIntegration
		: IControlMediaPlayer
	{
		#region IMediaPlayer Members

		/// <summary>
		/// Gets whether or not the media player is currently running.
		/// </summary>
		public bool IsRunning
		{
			get { return Process.GetProcessesByName ("itunes").Any(); }
		}

		public string SongName
		{
			get
			{
				try
				{
					var track = iTunes.CurrentTrack;
					return track != null ? track.Name : String.Empty;
				}
				catch (COMException)
				{
					return String.Empty;
				}
			}
		}

		public string ArtistName
		{
			get
			{
				try
				{
					var track = iTunes.CurrentTrack;
					return track != null ? track.Artist : String.Empty;
				}
				catch (COMException)
				{
					return String.Empty;
				}
			}
		}

		public string AlbumName
		{
			get
			{
				try
				{
					var track = iTunes.CurrentTrack;
					return track != null ? track.Album : String.Empty;
				}
				catch (COMException)
				{
					return String.Empty;
				}
			}
		}

		public int Volume
		{
			get
			{
				try
				{
					return iTunes.SoundVolume;
				}
				catch (COMException)
				{
					return 0;
				}
			}

			set
			{
				try
				{
					iTunes.SoundVolume = value;
				}
				catch (COMException)
				{
				}
			}
		}

		#endregion

		#region IControlMediaPlayer Members

		public void Play ()
		{
			try
			{
				iTunes.Play();
			}
			catch (COMException)
			{
			}
		}

		public void Pause ()
		{
			try
			{
				iTunes.Pause();
			}
			catch (COMException)
			{
			}
		}

		public void Stop ()
		{
			try
			{
				iTunes.Stop();
			}
			catch (COMException)
			{
			}
		}

		public void NextInPlaylist ()
		{
			try
			{
				iTunes.NextTrack();
			}
			catch (COMException)
			{
			}
		}

		public void PreviousInPlaylist ()
		{
			try
			{
				iTunes.PreviousTrack();
			}
			catch (COMException)
			{
			}
		}

		#endregion

		private iTunesAppClass itunes;
		private iTunesAppClass iTunes
		{
			get
			{
				if (itunes == null)
					itunes = new iTunesAppClass();

				return itunes;
			}
		}
	}
}