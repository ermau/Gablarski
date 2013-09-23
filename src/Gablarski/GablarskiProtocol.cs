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
using Gablarski.Messages;
using Tempest;

namespace Gablarski
{

	public enum GablarskiMessageType
		: ushort
	{
		Ping = 56,
		Connect = 1,
		Login = 3,
		Register = 42,
		Join = 37,
		Disconnect = 5,
		QueryServer = 34,
		PunchThrough = 39,
		
		SetComment = 47,
		SetStatus = 48,

		RequestSourceList = 15,
		RequestSource = 7,
		ClientAudioSourceStateChange = 28,
		RemoveSource = 26,

		RequestServerInfo = 11,
		RequestUserList = 13,
		
		RequestChannelList = 18,
		ChannelChange = 20,
		ChannelEdit = 22,
		
		RequestMuteUser = 44,
		RequestMuteSource = 45,

		SetPermissions = 52,
		KickUser = 50,
		BanUser = 51,
		RegistrationApproval = 54,

		ConnectionRejected = 2,
		PunchThroughReceived = 40,
		Redirect = 41,
		RegisterResult = 43,
		
		QueryServerResult = 36,
		ServerInfoReceived = 12,
		LoginResult = 4,
		JoinResult = 38,
		PermissionDenied = 32,

		SourceList = 16,
		SourceResult = 8,

		/// <summary>
		/// Audio data sent from a client.
		/// </summary>
		ClientAudioData = 10,

		/// <summary>
		/// Audio data sent from a server.
		/// </summary>
		ServerAudioData = 57,
		AudioSourceStateChange = 29,
		SourcesRemoved = 25,
		SourceMuted = 53,
	
		UserJoined = 24,
		Permissions = 33,
		UserInfoList = 14,
		UserList = 49,
		UserDisconnected = 17,
		UserChangedChannel = 27,
		UserMuted = 31,
		UserUpdated = 46,
		UserKicked = 55,

		ChannelList = 19,
		ChangeChannelResult = 21,
		ChannelEditResult = 23,

		JoinVoice = 58,
		JoinVoiceResponse = 59,
	} // next: 60

	public abstract class GablarskiMessage
		: Message
	{
		protected GablarskiMessage (GablarskiMessageType messageType)
			: base (GablarskiProtocol.Instance, (ushort)messageType)
		{
		}
	}

	public class GablarskiProtocol
	{
		public static readonly Protocol Instance = new Protocol (42, 7);
		public const int Port = 42912;

		static GablarskiProtocol()
		{
			Instance.Register (new List<KeyValuePair<Type, Func<Message>>> {
				new KeyValuePair<Type, Func<Message>> (typeof (ClientPingMessage), () => new ClientPingMessage()),
				new KeyValuePair<Type, Func<Message>> (typeof (ConnectMessage), () => new ConnectMessage()),
				new KeyValuePair<Type, Func<Message>> (typeof (LoginMessage), () => new LoginMessage()),
				new KeyValuePair<Type, Func<Message>> (typeof (RegisterMessage), () => new RegisterMessage()),
				new KeyValuePair<Type, Func<Message>> (typeof (JoinMessage), () => new JoinMessage()),
				new KeyValuePair<Type, Func<Message>> (typeof (DisconnectMessage), () => new DisconnectMessage()),
				new KeyValuePair<Type, Func<Message>> (typeof (QueryServerMessage), () => new QueryServerMessage()),
				new KeyValuePair<Type, Func<Message>> (typeof (PunchThroughMessage), () => new PunchThroughMessage()),
				new KeyValuePair<Type, Func<Message>> (typeof (SetCommentMessage), () => new SetCommentMessage()),
				new KeyValuePair<Type, Func<Message>> (typeof (SetStatusMessage), () => new SetStatusMessage()),
				new KeyValuePair<Type, Func<Message>> (typeof (RequestSourceListMessage), () => new RequestSourceListMessage()),
				new KeyValuePair<Type, Func<Message>> (typeof (RequestSourceMessage), () => new RequestSourceMessage()),
				new KeyValuePair<Type, Func<Message>> (typeof (ClientAudioSourceStateChangeMessage), () => new ClientAudioSourceStateChangeMessage()),
				new KeyValuePair<Type, Func<Message>> (typeof (RequestUserListMessage), () => new RequestUserListMessage()),
				new KeyValuePair<Type, Func<Message>> (typeof (RequestChannelListMessage), () => new RequestChannelListMessage()),
				new KeyValuePair<Type, Func<Message>> (typeof (ChannelChangeMessage), () => new ChannelChangeMessage()),
				new KeyValuePair<Type, Func<Message>> (typeof (ChannelEditMessage), () => new ChannelEditMessage()),
				new KeyValuePair<Type, Func<Message>> (typeof (RequestMuteUserMessage), () => new RequestMuteUserMessage()),
				new KeyValuePair<Type, Func<Message>> (typeof (RequestMuteSourceMessage), () => new RequestMuteSourceMessage()),
				new KeyValuePair<Type, Func<Message>> (typeof (SetPermissionsMessage), () => new SetPermissionsMessage()),
				new KeyValuePair<Type, Func<Message>> (typeof (KickUserMessage), () => new KickUserMessage()),
				new KeyValuePair<Type, Func<Message>> (typeof (BanUserMessage), () => new BanUserMessage()),
				new KeyValuePair<Type, Func<Message>> (typeof (RegistrationApprovalMessage), () => new RegistrationApprovalMessage()),
				new KeyValuePair<Type, Func<Message>> (typeof (ConnectionRejectedMessage), () => new ConnectionRejectedMessage()),
				new KeyValuePair<Type, Func<Message>> (typeof (PunchThroughReceivedMessage), () => new PunchThroughReceivedMessage()),
				new KeyValuePair<Type, Func<Message>> (typeof (RedirectMessage), () => new RedirectMessage()),
				new KeyValuePair<Type, Func<Message>> (typeof (RegisterResultMessage), () => new RegisterResultMessage()),
				new KeyValuePair<Type, Func<Message>> (typeof (QueryServerResultMessage), () => new QueryServerResultMessage()),
				new KeyValuePair<Type, Func<Message>> (typeof (ServerInfoMessage), () => new ServerInfoMessage()),
				new KeyValuePair<Type, Func<Message>> (typeof (LoginResultMessage), () => new LoginResultMessage()),
				new KeyValuePair<Type, Func<Message>> (typeof (JoinResultMessage), () => new JoinResultMessage()),
				new KeyValuePair<Type, Func<Message>> (typeof (PermissionDeniedMessage), () => new PermissionDeniedMessage()),
				new KeyValuePair<Type, Func<Message>> (typeof (SourceListMessage), () => new SourceListMessage()),
				new KeyValuePair<Type, Func<Message>> (typeof (SourceResultMessage), () => new SourceResultMessage()),
				new KeyValuePair<Type, Func<Message>> (typeof (ClientAudioDataMessage), () => new ClientAudioDataMessage()),
				new KeyValuePair<Type, Func<Message>> (typeof (ServerAudioDataMessage), () => new ServerAudioDataMessage()),
				new KeyValuePair<Type, Func<Message>> (typeof (AudioSourceStateChangeMessage), () => new AudioSourceStateChangeMessage()),
				new KeyValuePair<Type, Func<Message>> (typeof (SourcesRemovedMessage), () => new SourcesRemovedMessage()),
				new KeyValuePair<Type, Func<Message>> (typeof (SourceMutedMessage), () => new SourceMutedMessage()),
				new KeyValuePair<Type, Func<Message>> (typeof (UserJoinedMessage), () => new UserJoinedMessage()),
				new KeyValuePair<Type, Func<Message>> (typeof (PermissionsMessage), () => new PermissionsMessage()),
				new KeyValuePair<Type, Func<Message>> (typeof (UserInfoListMessage), () => new UserInfoListMessage()),
				new KeyValuePair<Type, Func<Message>> (typeof (UserListMessage), () => new UserListMessage()),
				new KeyValuePair<Type, Func<Message>> (typeof (UserDisconnectedMessage), () => new UserDisconnectedMessage()),
				new KeyValuePair<Type, Func<Message>> (typeof (UserChangedChannelMessage), () => new UserChangedChannelMessage()),
				new KeyValuePair<Type, Func<Message>> (typeof (UserMutedMessage), () => new UserMutedMessage()),
				new KeyValuePair<Type, Func<Message>> (typeof (UserUpdatedMessage), () => new UserUpdatedMessage()),
				new KeyValuePair<Type, Func<Message>> (typeof (UserKickedMessage), () => new UserKickedMessage()),
				new KeyValuePair<Type, Func<Message>> (typeof (ChannelListMessage), () => new ChannelListMessage()),
				new KeyValuePair<Type, Func<Message>> (typeof (ChannelChangeResultMessage), () => new ChannelChangeResultMessage()),
				new KeyValuePair<Type, Func<Message>> (typeof (ChannelEditResultMessage), () => new ChannelEditResultMessage()),
				new KeyValuePair<Type, Func<Message>> (typeof (JoinVoiceMessage), () => new JoinVoiceMessage()),
				new KeyValuePair<Type, Func<Message>> (typeof (JoinVoiceResponseMessage), () => new JoinVoiceResponseMessage())
			});
		}
	}
}