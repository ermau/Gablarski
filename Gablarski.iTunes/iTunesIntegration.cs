using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gablarski.Clients;
using iTunesLib;

namespace Gablarski.iTunes
{
	public class iTunesIntegration
		: IControlMediaPlayer
	{
		#region IMediaPlayer Members

		public string SongName
		{
			get
			{
				var track = itunes.CurrentTrack;
				if (track != null)
					return track.Name;

				return String.Empty;
			}
		}

		public string ArtistName
		{
			get
			{
				var track = itunes.CurrentTrack;
				if (track != null)
					return track.Artist;

				return String.Empty;
			}
		}

		public string AlbumName
		{
			get
			{
				var track = itunes.CurrentTrack;
				if (track != null)
					return track.Album;

				return String.Empty;
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