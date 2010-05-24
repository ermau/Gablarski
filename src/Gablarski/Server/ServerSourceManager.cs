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

namespace Gablarski.Server
{
	public class ServerSourceManager
		: AudioSourceManager, IServerSourceManager
	{
		private readonly IServerContext context;

		public ServerSourceManager (IServerContext serverContext)
		{
			this.context = serverContext;
		}

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