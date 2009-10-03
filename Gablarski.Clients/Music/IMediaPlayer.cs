using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gablarski.Clients.Music
{
	/// <summary>
	/// Provides a media-player integration contract.
	/// </summary>
	public interface IMediaPlayer
	{
		/// <summary>
		/// Gets whether or not the media player is currently running.
		/// </summary>
		bool IsRunning { get; }

		/// <summary>
		/// Gets the currently playing song name.
		/// </summary>
		string SongName { get; }

		/// <summary>
		/// Gets the currently playing artist name.
		/// </summary>
		string ArtistName { get; }

		/// <summary>
		/// Gets the currently playing album name.
		/// </summary>
		string AlbumName { get; }

		/// <summary>
		/// Sets the volume	for the media player. 0 - 100.
		/// </summary>
		/// <exception cref="ArgumentOutOfRangeException">When value is < 0 or > 100.</exception>
		int Volume { get; set; }
	}

	public class MediaPlayerException
		: ApplicationException
	{
		public MediaPlayerException()
		{
		}

		public MediaPlayerException (string message)
			: base (message)
		{
		}

		public MediaPlayerException (string message, Exception innerException)
			: base (message, innerException)
		{
		}
	}
}