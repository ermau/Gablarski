using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gablarski.Audio
{
	public interface IAudioReceiver
	{
		event EventHandler<AudioSourceEventArgs> AudioSourceStarted;
		event EventHandler<AudioSourceEventArgs> AudioSourceStopped;
		event EventHandler<ReceivedAudioEventArgs> ReceivedAudio;
	}

	public class AudioSourceEventArgs
		: EventArgs
	{
		public AudioSourceEventArgs (AudioSource source)
		{
			this.Source = source;
		}

		/// <summary>
		/// Gets the media source audio was received for.
		/// </summary>
		public AudioSource Source
		{
			get;
			private set;
		}
	}

	public class ReceivedAudioEventArgs
		: AudioSourceEventArgs
	{
		public ReceivedAudioEventArgs (AudioSource source, int sequence, byte[] data)
			: base (source)
		{
			this.AudioData = data;
			this.Sequence = sequence;
		}

		/// <summary>
		/// Gets the audio data.
		/// </summary>
		public byte[] AudioData
		{
			get;
			private set;
		}
		
		public int Sequence
		{
			get;
			private set;
		}
	}
}