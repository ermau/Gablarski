// Copyright (c) 2010-2013, Eric Maupin
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
using System.Linq;
using Gablarski.Client;

namespace Gablarski.Audio
{
	/// <summary>
	/// Contract for receiving audio and audio sources.
	/// </summary>
	public interface IAudioReceiver
	{
		/// <summary>
		/// An audio source was muted or unmuted.
		/// </summary>
		event EventHandler<AudioSourceMutedEventArgs> AudioSourceMuted;

		/// <summary>
		/// An audio source started playing.
		/// </summary>
		event EventHandler<AudioSourceEventArgs> AudioSourceStarted;

		/// <summary>
		/// An audio source stopped playing.
		/// </summary>
		event EventHandler<AudioSourceEventArgs> AudioSourceStopped;

		/// <summary>
		/// Audio data was received.
		/// </summary>
		event EventHandler<ReceivedAudioEventArgs> ReceivedAudio;

		/// <summary>
		/// Forces an unencoded audio receive from <paramref name="source"/> with <paramref name="audio"/>
		/// </summary>
		/// <param name="audio">Unencoded audio in the <paramref name="source"/>'s format.</param>
		void Receive (AudioSource source, byte[] audio);
	}

	public class AudioSourceEventArgs
		: EventArgs
	{
		public AudioSourceEventArgs (AudioSource source)
		{
			Source = source;
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
		public ReceivedAudioEventArgs (AudioSource source, byte[][] data)
			: base (source)
		{
			AudioData = data;
		}

		/// <summary>
		/// Gets the audio data.
		/// </summary>
		public byte[][] AudioData
		{
			get;
			private set;
		}
	}
}