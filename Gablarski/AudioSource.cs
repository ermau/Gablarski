using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gablarski
{
	public abstract class AudioSource
	{
		public IUser Owner
		{
			get; internal set;
		}

		public uint ID
		{
			get; internal set;
		}

		public AudioSourceChannels Channels
		{
			get; set;
		}
	}

	public enum AudioSourceChannels
	{
		Mono,
		Stereo
	}
}