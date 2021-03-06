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
using Gablarski.Audio;

namespace Gablarski
{
	/// <summary>
	/// Base contract for audio source managers.
	/// </summary>
	public interface ISourceManager
		: IIndexedEnumerable<int, AudioSource>
	{
		/// <summary>
		/// Gets the audio sources owned by <paramref name="user"/>.
		/// </summary>
		/// <param name="user">The owner to find sources for.</param>
		/// <returns>An empty enumerable if <paramref name="user"/> doesn't own any sources, otherwise the owned sources.</returns>
		IEnumerable<AudioSource> this[IUserInfo user] { get; }

		/// <summary>
		/// Removes the audio <paramref name="source"/>.
		/// </summary>
		/// <param name="source">The source to remove.</param>
		/// <returns><c>true</c> if the source was found and removed, <c>false otherwise.</c></returns>
		/// <exception cref="ArgumentNullException"><paramref name="source"/> is <c>null</c>.</exception>
		bool Remove (AudioSource source);

		/// <summary>
		/// Removes all audio sources for <paramref name="user"/>.
		/// </summary>
		/// <param name="user">The user to remove all sources for.</param>
		/// <returns><c>true</c> if any sources belonging to <paramref name="user"/> were found.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="user"/> is <c>null</c>.</exception>
		bool Remove (IUserInfo user);

		/// <summary>
		/// Toggles mute for the <paramref name="source"/>.
		/// </summary>
		/// <param name="source">The source to toggle mute for.</param>
		/// <returns><c>true</c> if <paramref name="source"/> was muted, <c>false</c> if not or not found.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="source"/> is <c>null</c>.</exception>
		bool ToggleMute (AudioSource source);

		/// <summary>
		/// Clears the manager of all sources.
		/// </summary>
		void Clear();
	}
}