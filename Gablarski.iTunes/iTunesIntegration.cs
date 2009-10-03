using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Gablarski.Clients;
using Gablarski.Clients.Music;
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
				var track = itunes.CurrentTrack;
				return track != null ? track.Name : String.Empty;
			}
		}

		public string ArtistName
		{
			get
			{
				var track = itunes.CurrentTrack;
				return track != null ? track.Artist : String.Empty;
			}
		}

		public string AlbumName
		{
			get
			{
				var track = itunes.CurrentTrack;
				return track != null ? track.Album : String.Empty;
			}
		}

		public int Volume
		{
			get { return itunes.SoundVolume; }
			set { itunes.SoundVolume = value; }
		}

		#endregion

		#region IControlMediaPlayer Members

		public void Play ()
		{
			itunes.Play();
		}

		public void Pause ()
		{
			itunes.Pause();
		}

		public void Stop ()
		{
			itunes.Stop();
		}

		public void NextTrack ()
		{
			itunes.NextTrack();
		}

		public void PreviousTrack ()
		{
			itunes.PreviousTrack();
		}

		#endregion

		private readonly iTunesAppClass itunes = new iTunesAppClass();
	}
}