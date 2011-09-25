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

namespace Gablarski.Audio
{
	public interface IAudioPlaybackProvider
		: IAudioDeviceProvider
	{
		/// <summary>
		/// Fired when a source finishes playing.
		/// </summary>
		/// <exception cref="ObjectDisposedException">This playback provider has already been disposed.</exception>
		event EventHandler<AudioSourceEventArgs> SourceFinished;

		/// <summary>
		/// Gets or sets the playback device.
		/// </summary>
		/// <exception cref="ObjectDisposedException">This playback provider has already been disposed.</exception>
		IAudioDevice Device { get; set; }

		/// <summary>
		/// Overall gain.
		/// </summary>
		/// <exception cref="ObjectDisposedException">This playback provider has already been disposed.</exception>
		float Gain { get; set; }

		/// <summary>
		/// Sets the gain for <paramref name="source"/>.
		/// </summary>
		/// <param name="source">The source to set the gain for.</param>
		/// <exception cref="ArgumentNullException"><paramref name="source"/> is <c>null</c>.</exception>
		void SetGain (AudioSource source, float gain);

		/// <summary>
		/// Opens the playback provider doing any one time initialization required.
		/// </summary>
		/// <exception cref="InvalidOperationException"><see cref="Device"/> is not set.</exception>
		/// <exception cref="ObjectDisposedException">This playback provider has already been disposed.</exception>
		void Open();
		
		/// <summary>
		/// Queues audio <paramref name="data"/> to be played back, owned by <paramref name="source"/>.
		/// </summary>
		/// <param name="source">The <see cref="AudioSource"/> the audio came from.</param>
		/// <param name="data">LPCM data.</param>
		/// <exception cref="ArgumentNullException"><paramref name="source"/> is <c>null</c>.</exception>
		/// <exception cref="ObjectDisposedException">This playback provider has already been disposed.</exception>
		void QueuePlayback (AudioSource source, byte[] data);

		/// <summary>
		/// Frees any internal resources associated with the <paramref name="source"/>.
		/// </summary>
		/// <param name="source">The source to free any resources for.</param>
		/// <exception cref="ArgumentNullException"><paramref name="source"/> is <c>null</c>.</exception>
		/// <exception cref="ObjectDisposedException">This playback provider has already been disposed.</exception>
		void FreeSource (AudioSource source);
	}

	public static class PlaybackProviderExtensions
	{
		/// <summary>
		/// Opens the provider with <paramref name="device"/>.
		/// </summary>
		/// <param name="device">The device to play audio on.</param>
		/// <exception cref="ArgumentNullException"><paramref name="device"/> is <c>null</c>.</exception>
		public static void Open (this IAudioPlaybackProvider self, IAudioDevice device)
		{
			if (self == null)
				throw new ArgumentNullException ("self");
			if (device == null)
				throw new ArgumentNullException ("device");

			self.Device = device;
			self.Open();
		}
	}
}