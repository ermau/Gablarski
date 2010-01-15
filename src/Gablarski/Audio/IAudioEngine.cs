// Copyright (c) 2009, Eric Maupin
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
using Gablarski.Client;

namespace Gablarski.Audio
{
	public enum AudioEngineCaptureMode
	{
		Explicit = 0,
		Activated
	}

	public class AudioEngineCaptureOptions
	{
		public AudioEngineCaptureMode Mode
		{
			get; set;
		}

		/// <summary>
		/// The minimum volume required to start 'talking'.
		/// </summary>
		/// <remarks>abs(16bitsample - 128)</remarks>
		public int StartVolume
		{
			get; set;
		}

		/// <summary>
		/// The minimum volume required to continue 'talking'.
		/// </summary>
		/// <remarks>abs(16bitsample - 128)</remarks>
		public int ContinuationVolume
		{
			get; set;
		}

		/// <summary>
		/// Gets or sets a threshold time inbetween continuation volume matches to keep talking.
		/// </summary>
		public TimeSpan ContinueThreshold
		{
			get; set;
		}
	}

	public class AudioEnginePlaybackOptions
	{
		
	}

	public interface IAudioEngine
	{
		/// <summary>
		/// Gets whether the engine is currently running or not.
		/// </summary>
		bool IsRunning { get; }

		/// <summary>
		/// Gets or sets the client context for the engine.
		/// </summary>
		/// <exception cref="InvalidOperationException"><see cref="IsRunning"/> is <c>true</c>.</exception>
		IClientContext Context { get; set; }

		/// <summary>
		/// Gets or sets the audio receiver.
		/// </summary>
		/// <exception cref="InvalidOperationException"><see cref="IsRunning"/> is <c>true</c>.</exception>
		IAudioReceiver AudioReceiver { get; set; }

		/// <summary>
		/// Gets or sets the audio sender.
		/// </summary>
		/// <exception cref="InvalidOperationException"><see cref="IsRunning"/> is <c>true</c>.</exception>
		IAudioSender AudioSender { get; set; }

		/// <summary>
		/// Attaches a playback provider to all <paramref name="sources"/> not already attached, skipping any ClientAudioSources.
		/// </summary>
		void Attach (IPlaybackProvider playback, IEnumerable<AudioSource> sources, AudioEnginePlaybackOptions options);

		/// <summary>
		/// Attaches a playback provider to be used for the given source.
		/// </summary>
		void Attach (IPlaybackProvider playback, AudioSource source, AudioEnginePlaybackOptions options);

		/// <summary>
		/// Starts a capture with the given <paramref name="capture"/> pumped to the <paramref name="source"/> with the given <paramref name="options"/>.
		/// </summary>
		/// <param name="capture">The provider to pump the audio from. (If the device is not preselected, the default device will be used.)</param>
		/// <param name="source">The audio source to pump the audio to.</param>
		/// <param name="options">Capturing options.</param>
		/// <exception cref="ArgumentNullException"><paramref name="capture"/> or <paramref name="source"/> or <paramref name="options"/> are <c>null</c>.</exception>
		void Attach (ICaptureProvider capture, AudioFormat format, AudioSource source, AudioEngineCaptureOptions options);

		/// <summary>
		/// Stops any captures on the given provider.
		/// </summary>
		/// <param name="provider">The provider to stop any captures for.</param>
		/// <returns><c>true</c> if there were any captures for the <paramref name="provider"/>.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="provider"/> is <c>null</c>.</exception>
		bool Detach (ICaptureProvider provider);

		/// <summary>
		/// Stops any playbacks on the given provider.
		/// </summary>
		/// <param name="provider">The provider to stop playback for.</param>
		/// <returns><c>true</c> if any sources were attached with <paramref name="provider"/>, <c>false</c> otherwise.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="provider"/> is <c>null</c>.</exception>
		bool Detach (IPlaybackProvider provider);

		/// <summary>
		/// Stops any playback or capturing to <paramref name="source"/>.
		/// </summary>
		/// <param name="source">The source to stop any playback or capturing for.</param>
		/// <returns><c>true</c> if any playback or capturing was occuring for <paramref name="source"/>, <c>false</c> otherwise.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="source"/> is <c>null</c>.</exception>
		bool Detach (AudioSource source);

		/// <exception cref="ArgumentNullException"><paramref name="capture"/> is <c>null</c>.</exception>
		void Mute (ICaptureProvider capture);
		/// <exception cref="ArgumentNullException"><paramref name="capture"/> is <c>null</c>.</exception>
		void Unmute (ICaptureProvider capture);

		/// <exception cref="ArgumentNullException"><paramref name="playback"/> is <c>null</c>.</exception>
		void Mute (IPlaybackProvider playback);
		/// <exception cref="ArgumentNullException"><paramref name="playback"/> is <c>null</c>.</exception>
		void Unmute (IPlaybackProvider playback);

		/// <summary>
		/// Starts the audio engine.
		/// </summary>
		/// <exception cref="System.InvalidOperationException"><see cref="AudioSender"/>, <see cref="AudioReceiver"/> or <see cref="Context"/> are not set.</exception>
		void Start();

		/// <summary>
		/// Stops the audio engine and clears all attachments.
		/// </summary>
		void Stop();

		/// <summary>
		/// Begins an explicit capture for <paramref name="source"/>.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="channels"></param>
		/// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="channels"/> are <c>null</c>.</exception>
		void BeginCapture (AudioSource source, IEnumerable<ChannelInfo> channels);
		/// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="users"/> are <c>null</c>.</exception>
		void BeginCapture (AudioSource source, IEnumerable<UserInfo> users);
		
		void EndCapture (AudioSource source);
	}

	public static class AudioEngineExtensions
	{
		public static void BeginCapture (this IAudioEngine self, AudioSource source, ChannelInfo channel)
		{
			if (self == null)
				throw new ArgumentNullException ("self");

			self.BeginCapture (source, new[] { channel });
		}

		public static void BeginCapture (this IAudioEngine self, AudioSource source, UserInfo user)
		{
			if (self == null)
				throw new ArgumentNullException ("self");

			self.BeginCapture (source, new[] { user });
		}
	}
}