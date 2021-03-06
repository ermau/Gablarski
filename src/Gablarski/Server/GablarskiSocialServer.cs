﻿// Copyright (c) 2013, Eric Maupin
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
using Tempest;
using Tempest.Social;

namespace Gablarski.Server
{
	public class GablarskiSocialServer
		: SocialServer
	{
		public GablarskiSocialServer (IWatchListProvider provider, IIdentityProvider identityProvider)
			: base (provider, identityProvider)
		{
			this.RegisterMessageHandler<JoinVoiceMessage> (OnJoinVoiceMessage);
		}

		private async void OnJoinVoiceMessage (MessageEventArgs<JoinVoiceMessage> e)
		{
			var person = await GetPersonAsync (e.Connection);
			if (person == null) {
				await e.Connection.DisconnectAsync();
				return;
			}

			Group group;
			lock (SyncRoot) {
				if (!Groups.TryGetGroup (e.Message.GroupId, out group) || !group.Participants.Contains (person.Identity))
					return;
			}

			IConnection connection;
			lock (SyncRoot) {
				connection = GetConnection (group.OwnerId);
			}

			if (connection == null)
				return;

			var join = new JoinVoiceMessage {
				GroupId = e.Message.GroupId,
				Target = e.Message.Target
			};

			try {
				var response = await connection.SendFor<JoinVoiceResponseMessage> (join, responseTimeout: 30000);
				await e.Connection.SendResponseAsync (e.Message, response);
			} catch (OperationCanceledException) {
			}
		}
	}
}