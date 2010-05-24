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
using System.IO;
using System.Linq;
using Gablarski.Client;

namespace Gablarski.Clients.CLI
{
	public abstract class GClientModule
		: CommandModule
	{
		private readonly TextWriter writer;
		private readonly GablarskiClient client;

		protected GClientModule (GablarskiClient client, TextWriter writer)
		{
			this.client = client;
			this.writer = writer;
		}

		protected GablarskiClient Client
		{
			get { return this.client; }
		}

		protected TextWriter Writer
		{
			get { return this.writer; }
		}

		protected IUserInfo FindUser (string part)
		{
			int userId;
			return (Int32.TryParse (part, out userId))
			       	? Client.Users[userId]
			       	: Client.Users.FirstOrDefault (u => u.Nickname.Trim().ToLower() == part.Trim().ToLower());
		}

		protected ChannelInfo FindChannel (string part)
		{
			int channelId;
			return (Int32.TryParse (part, out channelId))
			       	? Client.Channels[channelId]
			       	: Client.Channels.FirstOrDefault (c => c.Name.Trim().ToLower() == part.Trim().ToLower());
		}
	}
}