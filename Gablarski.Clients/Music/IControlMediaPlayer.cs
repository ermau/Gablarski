using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gablarski.Clients.Music
{
	public interface IControlMediaPlayer
		: IMediaPlayer
	{
		void Play();
		void Pause();
		void Stop();
		void NextTrack();
		void PreviousTrack();
	}
}