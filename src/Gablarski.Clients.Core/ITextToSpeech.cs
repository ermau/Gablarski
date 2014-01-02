// Copyright (c) 2011-2014, Eric Maupin
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
using System.Threading.Tasks;
using Gablarski.Audio;
using Gablarski.Clients.Media;

namespace Gablarski.Clients
{
	/// <summary>
	/// TTS Contract
	/// </summary>
	public interface ITextToSpeech
		: INamedComponent, IDisposable
	{
		/// <summary>
		/// Gets the supported audio formats of this TTS engine.
		/// </summary>
		IEnumerable<AudioFormat> SupportedFormats { get; }

		/// <summary>
		/// Sets the media controller to use for this TTS engine.
		/// </summary>
		IMediaController Media { set; }

		/// <summary>
		/// Sets the audio source to use for this TTS engine.
		/// </summary>
		/// <exception cref="ArgumentException">The audio format of the source is not compatible with any in <see cref="SupportedFormats"/>.</exception>
		AudioSource AudioSource { get; set; }

		/// <summary>
		/// Tells the text to speech engine what to say.
		/// </summary>
		/// <param name="say"></param>
		Task SayAsync (string say);

		/// <summary>
		/// Gets the audio for <paramref name="say"/>.
		/// </summary>
		byte[] GetSpeech (string say);
	}
}