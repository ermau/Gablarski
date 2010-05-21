// Copyright (c) 2010, Eric Maupin
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
	/// <summary>
	/// Contract for client-side AudioSource state managers.
	/// </summary>
	public interface IClientSourceManager
		: ISourceManager
	{
		object SyncRoot { get; }

		/// <summary>
		/// Adds <paramref name="source"/> to the manager.
		/// </summary>
		/// <param name="source"></param>
		/// <exception cref="ArgumentNullException"><paramref name="source"/> is <c>null</c>.</exception>
		void Add (AudioSource source);

		/// <summary>
		/// Updates the manager with a new list of sources.
		/// </summary>
		/// <param name="sources">The new list of sources.</param>
		/// <exception cref="ArgumentNullException"><paramref name="sources"/> is <c>null</c>.</exception>
		void Update (IEnumerable<AudioSource> sources);

		/// <summary>
		/// Updates the manager with new state for a source.
		/// </summary>
		/// <param name="source">The source to update.</param>
		/// <exception cref="ArgumentNullException"><paramref name="source"/> is <c>null</c>.</exception>
		void Update (AudioSource source);

		/// <summary>
		/// Gets whether the source is ignored or not..
		/// </summary>
		/// <param name="source">The source to check.</param>
		/// <returns><c>true</c> if the source is ignored.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="source"/> is <c>null</c>.</exception>
		bool GetIsIgnored (AudioSource source);

		/// <summary>
		/// Toggles ignore for the source.
		/// </summary>
		/// <param name="source">The source to ignore.</param>
		/// <returns>The new state of ignore on <paramref name="source"/>.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="source"/> is <c>null</c>.</exception>
		bool ToggleIgnore (AudioSource source);
	}
}