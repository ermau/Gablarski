// Copyright (c) 2010, Eric Maupin
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
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using Gablarski.Clients.Media;
using iTunesLib;

namespace Gablarski.iTunes
{
	[Export (typeof(IMediaPlayer))]
	[Export (typeof(IControlMediaPlayer))]
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

		public string Name
		{
			get { return "iTunes"; }
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
				catch (InvalidCastException)
				{
					return String.Empty;
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
				catch (InvalidCastException)
				{
					return String.Empty;
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
				catch (InvalidCastException)
				{
					return String.Empty;
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
				catch (InvalidCastException)
				{
					return 0;
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
				catch (InvalidCastException)
				{
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
			catch (InvalidCastException)
			{
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
			catch (InvalidCastException)
			{
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
			catch (InvalidCastException)
			{
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
			catch (InvalidCastException)
			{
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
			catch (InvalidCastException)
			{
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