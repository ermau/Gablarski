// Copyright (c) 2009-2014, Eric Maupin
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
using Gablarski.Audio;

namespace Gablarski.Server
{
	public sealed class ServerSourceManager
		: AudioSourceManager
	{
		/// <summary>
		/// Creates a new audio source
		/// </summary>
		/// <param name="name">The name of the source as requested by the user.</param>
		/// <param name="owner">The user id that owns the audio source.</param>
		/// <param name="audioArgs">The audio properties of the source to create.</param>
		/// <returns>The newly created audio source.</returns>
		/// <exception cref="ArgumentException"><paramref name="name"/> is in use by the user already.</exception>
		/// <exception cref="ArgumentNullException"><paramref name="name"/>, <paramref name="owner"/> or <paramref name="audioArgs"/> is <c>null</c>.</exception>
		public AudioSource Create (string name, IUserInfo owner, AudioCodecArgs audioArgs)
		{
			if (name == null)
				throw new ArgumentNullException ("name");
			if (owner == null)
				throw new ArgumentNullException ("owner");
			if (audioArgs == null)
				throw new ArgumentNullException ("audioArgs");

			if (OwnedSources.Contains (owner.UserId))
			{
				if (OwnedSources[owner.UserId].Any (s => s.Name == name))
					throw new ArgumentException ("Duplicate source name", "name");
			}

			int id = 1;
			if (Sources.Keys.Any())
				id = Sources.Keys.Max() + 1;

			var source = new AudioSource (name, id, owner.UserId, audioArgs);

			Sources.Add (source.Id, source);
			OwnedSources.Add (owner.UserId, source);

			return source;
		}

		/// <summary>
		/// Gets whether the <paramref name="sourceName"/> is in use by <paramref name="user"/>.
		/// </summary>
		/// <param name="user">The user to check the sources of.</param>
		/// <param name="sourceName">The name to check for.</param>
		/// <returns><c>true</c> if the source name is in use, <c>false</c> if not or <paramref name="user"/> wasn't found.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="user"/> or <paramref name="sourceName"/> is <c>null</c>.</exception>
		public bool IsSourceNameTaken (IUserInfo user, string sourceName)
		{
			if (user == null)
				throw new ArgumentNullException ("user");
			if (sourceName == null)
				throw new ArgumentNullException ("sourceName");

			return (OwnedSources.Contains (user.UserId) && (OwnedSources[user.UserId].Any (s => s.Name == sourceName)));
		}
	}
}