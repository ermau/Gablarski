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
using System.Linq;
using System.Text;

namespace Gablarski.Clients.Media
{
	/// <summary>
	/// Provides a media-player integration contract.
	/// </summary>
	public interface IMediaPlayer
		: INamedComponent
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
		/// <exception cref="ArgumentOutOfRangeException">When value is &lt; 0 or &gt; 100.</exception>
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