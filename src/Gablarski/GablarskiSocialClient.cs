// Copyright (c) 2013, Eric Maupin
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
using System.Threading;
using System.Threading.Tasks;
using Gablarski.Messages;
using Tempest;
using Tempest.Providers.Network;
using Tempest.Social;

namespace Gablarski
{
	public class GablarskiSocialClient
		: SocialClient
	{
		public GablarskiSocialClient (Person person, RSAAsymmetricKey key)
			: base (new NetworkClientConnection (new [] { SocialProtocol.Instance, GablarskiProtocol.Instance }, key), person)
		{
			if (key == null)
				throw new ArgumentNullException ("key");
		}

		public void SetTarget (Target target)
		{
			if (target == null)
				throw new ArgumentNullException ("target");

			this.target = target;

			Reconnect();
		}

		public async Task<Group> StartGroupWithAsync (Person person)
		{
			if (person == null)
				throw new ArgumentNullException ("person");

			Group group = await CreateGroupAsync().ConfigureAwait (false);
			if (group == null)
				return null;

			Invitation invitation = await InviteToGroupAsync (group, person).ConfigureAwait (false);
			if (invitation == null)
				return null;

			return group;
		}

		public async Task<Target> RequestGroupVoiceAsync (Group group)
		{
			if (group == null)
				throw new ArgumentNullException ("group");

			var joinVoice = new JoinVoiceMessage {
				Target = ServerTarget,
				GroupId = group.Id
			};

			JoinVoiceMessage join = await Connection.SendFor<JoinVoiceMessage> (joinVoice, 30000).ConfigureAwait(false);
			return join.Target;
		}

		private Target target;

		private int forwardedId;

		protected override void OnDisconnected (ClientDisconnectedEventArgs e)
		{
			base.OnDisconnected (e);

			Task.Run (() => Reconnect());
		}

		private async Task Reconnect()
		{
			ClientConnectionResult result;
			do {
				result = await ConnectAsync (this.target).ConfigureAwait (false);
				await Task.Delay (10000).ConfigureAwait (false);
			} while (result.Result != ConnectionResult.Success);
		}
	}
}