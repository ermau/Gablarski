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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cadenza.Collections;
using Gablarski.Audio;

namespace Gablarski
{
	public class SourceManager
		: ISourceManager
	{
		public IEnumerator<AudioSource> GetEnumerator()
		{
			return this.Sources.Values.ToList().GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public AudioSource this [int key]
		{
			get
			{
				AudioSource source;
				Sources.TryGetValue (key, out source);

				return source;
			}
		}

		public IEnumerable<AudioSource> this [UserInfo user]
		{
			get
			{
				return OwnedSources[user.UserId];
			}
		}


		public bool ToggleMute (AudioSource source)
		{
			if (source == null)
				throw new ArgumentNullException ("source");

			AudioSource actual;
			if (!Sources.TryGetValue (source.Id, out actual))
				return false;

			AudioSource newSource = new AudioSource (actual);
			newSource.IsMuted = !actual.IsMuted;

			OwnedSources.Remove (source.OwnerId, source);
			Sources[newSource.Id] = newSource;
			OwnedSources.Add (source.OwnerId, source);

			return newSource.IsMuted;
		}

		public void Remove (AudioSource source)
		{
			if (source == null)
				throw new ArgumentNullException ("source");

			OwnedSources.Remove (source.OwnerId, source);
			Sources.Remove (source.Id);
		}

		public void Remove (UserInfo user)
		{
			if (user == null)
				throw new ArgumentNullException ("user");

			foreach (AudioSource source in OwnedSources[user.UserId])
				Sources.Remove (source.Id);

			OwnedSources.Remove (user.UserId);
		}

		protected readonly MutableLookup<int, AudioSource> OwnedSources = new MutableLookup<int, AudioSource>();
		protected readonly Dictionary<int, AudioSource> Sources = new Dictionary<int, AudioSource>();
	}
}