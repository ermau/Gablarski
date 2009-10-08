// Copyright (c) 2009, Eric Maupin
// All rights reserved.

// Redistribution and use in source and binary forms, with
// or without modification, are permitted provided that
// the following conditions are met:

// - Redistributions of source code must retain the above 
//   copyright notice, this list of conditions and the
//   following disclaimer.

// - Redistributions in binary form must reproduce the above
//   copyright notice, this list of conditions and the
//   following disclaimer in the documentation and/or other
//   materials provided with the distribution.

// - Neither the name of Gablarski nor the names of its
//   contributors may be used to endorse or promote products
//   derived from this software without specific prior
//   written permission.

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
		event EventHandler<CaptureSourceStateChangedEventArgs> CaptureSourceStateChanged;

		IClientContext Context { get; set; }

		/// <summary>
		/// Gets or sets the audio receiver.
		/// </summary>
		IAudioReceiver AudioReceiver { get; set; }

		/// <summary>
		/// Gets or sets the audio sender.
		/// </summary>
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
		void Attach (ICaptureProvider capture, AudioFormat format, AudioSource source, AudioEngineCaptureOptions options);

		/// <summary>
		/// Stops any captures on the given provider.
		/// </summary>
		/// <param name="provider">The provider to stop any captures for.</param>
		/// <returns><c>true</c> if there were any captures for the <paramref name="provider"/>.</returns>
		bool Detach (ICaptureProvider provider);

		/// <summary>
		/// Stops any playback or capturing to <paramref name="source"/>.
		/// </summary>
		/// <param name="source">The source to stop any playback or capturing for.</param>
		/// <returns><c>true</c> if any playback or capturing was occuring for <paramref name="source"/>, <c>false</c> otherwise.</returns>
		bool Detach (AudioSource source);

		/// <summary>
		/// Starts the audio engine.
		/// </summary>
		void Start();

		/// <summary>
		/// Stops the audio engine and clears all attachments.
		/// </summary>
		void Stop();

		void BeginCapture (AudioSource source, ChannelInfo channel);
		void EndCapture (AudioSource source, ChannelInfo channel);
	}

	public class CaptureSourceStateChangedEventArgs
		: AudioSourceEventArgs
	{
		public CaptureSourceStateChangedEventArgs (AudioSource source, bool talking)
			: base (source)
		{
			this.Talking = talking;
		}

		public bool Talking { get; private set; }
	}
}
