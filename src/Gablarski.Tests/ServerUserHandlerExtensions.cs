// Copyright (c) 2014, Eric Maupin
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


using Gablarski.Messages;
using Gablarski.Server;
using NUnit.Framework;
using Tempest;

namespace Gablarski.Tests
{
	public static class ServerUserHandlerExtensions
	{
		public static void Connect (this ServerUserHandler self, IServerConnection serverConnection, ConnectionBuffer clientConnection)
		{
			self.OnConnectMessage (new MessageEventArgs<ConnectMessage> (serverConnection,
				new ConnectMessage {
					Host = "localhost",
					Port = GablarskiProtocol.Port,
					ProtocolVersion = GablarskiProtocol.Instance.Version
				}));

			clientConnection.DequeueAndAssertMessage<ServerInfoMessage>();
			clientConnection.DequeueAndAssertMessage<ChannelListMessage>();
			clientConnection.DequeueAndAssertMessage<UserInfoListMessage>();
			clientConnection.DequeueAndAssertMessage<SourceListMessage>();
		}

		public static void Join (this ServerUserHandler self, IServerConnection serverConnection, ConnectionBuffer clientConnection, ref IUserInfo user, bool isLoggedIn = false)
		{
			self.OnJoinMessage (new MessageEventArgs<JoinMessage> (serverConnection, new JoinMessage {
				Header = new MessageHeader(),
				Nickname = user.Nickname,
				Phonetic = user.Phonetic,
			}));

			var result = clientConnection.DequeueAndAssertMessage<JoinResultMessage>();
			Assert.That (result.Result, Is.EqualTo (LoginResultState.Success));
			user = result.UserInfo;

			if (!isLoggedIn)
				clientConnection.DequeueAndAssertMessage<PermissionsMessage>();
		}
	}
}