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
using Gablarski.Audio;

namespace Gablarski.Client
{
	public interface IClientSourceHandler
		: ISourceHandler<AudioSource>, IAudioReceiver, IAudioSender
	{
		/// <summary>
		/// A new  or updated source list has been received.
		/// </summary>
		event EventHandler<ReceivedListEventArgs<AudioSource>> ReceivedSourceList;

		/// <summary>
		/// A new audio source has been received.
		/// </summary>
		event EventHandler<ReceivedAudioSourceEventArgs> ReceivedAudioSource;

		/// <summary>
		/// An audio source was removed.
		/// </summary>
		event EventHandler<ReceivedListEventArgs<AudioSource>> AudioSourcesRemoved;

		/// <summary>
		/// Get's the current user's audio sources.
		/// </summary>
		IEnumerable<AudioSource> Mine { get; }

		/// <summary>
		/// Requests a source.
		/// </summary>
		/// <param name="targetBitrate">The target bitrate to request.</param>
		/// <param name="name">The user-local name of the source, used to identify the source later.</param>
		/// <remarks>
		/// The server may not agree with the bitrate you request, do not set up audio based on this
		/// target, but on the bitrate of the source you actually receive.
		/// </remarks>
		/// <exception cref="ArgumentNullException"><paramref name="name"/> or <paramref name="format"/> are <c>null</c>.</exception>
		void Request (string name, AudioFormat format, short frameSize, int targetBitrate);

		/// <summary>
		/// Creates a fake audio source for playing audio locally through the Gablarski audio engine.
		/// </summary>
		/// <param name="name">The user-local name of the source, used to identify the source later.</param>
		/// <param name="format">The format of the audio.</param>
		/// <param name="frameSize">The frame size of the audio.</param>
		/// <exception cref="ArgumentNullException"><paramref name="name"/> or <paramref name="format"/> are <c>null</c>.</exception>
		/// <remarks>
		/// Fake audio sources do not trigger or show up in <see cref="ReceivedSourceList"/> or <see cref="ReceivedAudioSource"/>
		/// </remarks>
		AudioSource CreateFake (string name, AudioFormat format, short frameSize);

		/// <summary>
		/// Gets whether or not the source is muted.
		/// </summary>
		/// <param name="source">The source to check.</param>
		/// <returns><c>true</c> if ignored, <c>false</c> if not.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="source"/> is <c>null</c></exception>
		bool GetIsIgnored (AudioSource source);

		/// <summary>
		/// Toggles ignore for the source.
		/// </summary>
		/// <param name="source">The source to ignore.</param>
		/// <returns>The new state of ignore on <paramref name="source"/>.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="source"/> is <c>null</c>.</exception>
		bool ToggleIgnore (AudioSource source);

		/// <summary>
		/// Toggles mute for the source.
		/// </summary>
		/// <param name="source">The source to mute.</param>
		/// <exception cref="ArgumentNullException"><paramref name="source"/> is <c>null</c>.</exception>
		void ToggleMute (AudioSource source);

		/// <summary>
		/// Resets the handler back to the starting state.
		/// </summary>
		void Reset();
	}

	public static class ClientSourceHandlerExtensions
	{
		public static void Request (this IClientSourceHandler self, string name, AudioFormat format, short frameSize)
		{
			if (self == null)
				throw new ArgumentNullException ("self");

			self.Request (name, format, frameSize, 0);
		}
	}
}