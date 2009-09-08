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
using Gablarski.Messages;

namespace Gablarski.Client
{
	public class ClientUser
		: UserInfo
	{
		internal ClientUser (IClientConnection client)
		{
			if (client == null)
				throw new ArgumentNullException ("client");

			this.client = client;
		}

		public ClientUser (UserInfo user, IClientConnection client)
			: base (user.Nickname, user.Username, user.UserId, user.CurrentChannelId, user.IsMuted)
		{
			if (client == null)
				throw new ArgumentNullException ("client");

			this.client = client;
		}

		public ClientUser (string nickname, int userId, int currentChannelId, IClientConnection client, bool muted)
			: base (nickname, nickname, userId, currentChannelId, muted)
		{
			if (client == null)
				throw new ArgumentNullException ("client");

			this.client = client;
		}

		public bool IsIgnored
		{
			get; private set;
		}

		/// <summary>
		/// Moves this user to <paramref name="targetChannel"/>.
		/// </summary>
		/// <param name="targetChannel">The channel to move this user to.</param>
		public void Move (ChannelInfo targetChannel)
		{
			if (targetChannel == null)
				throw new ArgumentNullException ("targetChannel");

			this.client.Send (new ChannelChangeMessage (this.UserId, targetChannel.ChannelId));
		}

		/// <summary>
		/// Toggles this user's ignored status.
		/// </summary>
		/// <returns><c>true</c> if the user is now ignored, <c>false</c> otherwise.</returns>
		public bool ToggleIgnore()
		{
			return this.IsIgnored = !this.IsIgnored;
		}

		public void ToggleMute ()
		{
			this.client.Send (new RequestMuteMessage { Target = this.Username, Type = MuteType.User, Unmute = !this.IsMuted });
		}

		private readonly IClientConnection client;
	}
}