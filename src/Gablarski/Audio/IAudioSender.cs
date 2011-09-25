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
using System.Linq;
using Gablarski.Messages;

namespace Gablarski.Audio
{
	public interface IAudioSender
	{
		/// <summary>
		/// Sends notifications that you're begining to send audio from <paramref name="source"/>
		/// </summary>
		/// <param name="source">The source to send from.</param>
		/// <exception cref="ArgumentNullException"><paramref name="source"/> is <c>null</c>.</exception>
		/// <exception cref="ArgumentException"><paramref name="source"/> does not belong to you.</exception>
		void BeginSending (AudioSource source);

		/// <summary>
		/// Sends a frame of audio data to the source.
		/// </summary>
		/// <param name="source">The source to send from.</param>
		/// <param name="targetType">The type that the <paramref name="targetIds"/> belong to.</param>
		/// <param name="targetIds">The ids of the targets to send audio to.</param>
		/// <param name="data">The unencoded LPCM matching <paramref name="source"/>'s attributes</param>
		/// <exception cref="ArgumentNullException"><paramref name="source"/> is <c>null</c>.</exception>
		/// <exception cref="ArgumentException"><paramref name="source"/> does not belong to you.</exception>
		/// <exception cref="ArgumentNullException"><paramref name="data"/> is <c>null</c>.</exception>
		/// <exception cref="ArgumentException"><paramref name="data"/> is empty.</exception>
		void SendAudioData (AudioSource source, TargetType targetType, int[] targetIds, byte[][] data);

		/// <summary>
		/// Sends notifications that you're finished sending audio from <paramref name="source"/>.
		/// </summary>
		/// <param name="source">The source to send from.</param>
		/// <exception cref="ArgumentNullException"><paramref name="source"/> is <c>null</c>.</exception>
		/// <exception cref="ArgumentException"><paramref name="source"/> does not belong to you.</exception>
		void EndSending (AudioSource source);
	}
}