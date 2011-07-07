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
using System.Reflection;
using System.Reflection.Emit;
using Cadenza.Collections;

namespace Gablarski.Messages
{
	public abstract class MessageBase
	{
		public abstract ushort MessageTypeCode
		{
			get;
		}

		public virtual bool Reliable
		{
			get { return true; }
		}

		public virtual bool AcceptedConnectionless
		{
			get { return false; }
		}

		public virtual bool Encrypted
		{
			get { return false; }
		}

		public virtual int MessageSize
		{
			get { return 0; }
		}

		public abstract void WritePayload (IValueWriter writer);
		public abstract void ReadPayload (IValueReader reader);

		public static bool GetMessage (ushort messageType, out MessageBase msg)
		{
			msg = null;
			Func<MessageBase> msgctor;
			if (MessageTypes.TryGetValue (messageType, out msgctor))
			{
				msg = msgctor();
				return true;
			}

			return false;
		}

		public static ReadOnlyDictionary<ushort, Func<MessageBase>> MessageTypes
		{
			get;
			private set;
		}

		static MessageBase ()
		{
			//#if SAFE

			MessageTypes = new ReadOnlyDictionary<ushort,Func<MessageBase>> (new Dictionary<ushort, Func<MessageBase>>
			{
				{ (ushort)ServerMessageType.AudioData, () => new ServerAudioDataMessage() },
			    { (ushort)ServerMessageType.AudioSourceStateChange, () => new AudioSourceStateChangeMessage() },
			    { (ushort)ServerMessageType.ChangeChannelResult, () => new ChannelChangeResultMessage() },
			    { (ushort)ServerMessageType.ChannelEditResult, () => new ChannelEditResultMessage() },
			    { (ushort)ServerMessageType.ChannelList, () => new ChannelListMessage() },
				{ (ushort)ServerMessageType.ConnectionRejected, () => new ConnectionRejectedMessage() },
				{ (ushort)ServerMessageType.Disconnect, () => new DisconnectMessage() },
				{ (ushort)ServerMessageType.JoinResult, () => new JoinResultMessage() },
				{ (ushort)ServerMessageType.LoginResult, () => new LoginResultMessage() },
				{ (ushort)ServerMessageType.PermissionDenied, () => new PermissionDeniedMessage() },
				{ (ushort)ServerMessageType.Permissions, () => new PermissionsMessage() },
				{ (ushort)ServerMessageType.PunchThroughReceived, () => new PunchThroughReceivedMessage() },
				{ (ushort)ServerMessageType.QueryServerResult, () => new QueryServerResultMessage() },
				{ (ushort)ServerMessageType.Redirect, () => new RedirectMessage() },
				{ (ushort)ServerMessageType.RegisterResult, () => new RegisterResultMessage() },
				{ (ushort)ServerMessageType.ServerInfoReceived, () => new ServerInfoMessage() },
			    { (ushort)ServerMessageType.SourceList, () => new SourceListMessage() },
				{ (ushort)ServerMessageType.SourceMuted, () => new SourceMutedMessage() },
			    { (ushort)ServerMessageType.SourceResult, () => new SourceResultMessage() },
			    { (ushort)ServerMessageType.SourcesRemoved, () => new SourcesRemovedMessage() },
			    { (ushort)ServerMessageType.UserChangedChannel, () => new UserChangedChannelMessage() },
			    { (ushort)ServerMessageType.UserDisconnected, () => new UserDisconnectedMessage() },
			    { (ushort)ServerMessageType.UserJoined, () => new UserJoinedMessage() },
				{ (ushort)ServerMessageType.UserInfoList, () => new UserInfoListMessage() },
				{ (ushort)ServerMessageType.UserKicked, () => new UserKickedMessage() },
				{ (ushort)ServerMessageType.UserList, () => new UserListMessage() },
				{ (ushort)ServerMessageType.UserMuted, () => new UserMutedMessage() },
				{ (ushort)ServerMessageType.UserUpdated, () => new UserUpdatedMessage() },
				{ (ushort)ServerMessageType.Ping, () => new ServerPingMessage() },

			    { (ushort)ClientMessageType.AudioData, () => new ClientAudioDataMessage() },
				{ (ushort)ClientMessageType.BanUser, () => new BanUserMessage() },
			    { (ushort)ClientMessageType.ChannelChange, () => new ChannelChangeMessage() },
				{ (ushort)ClientMessageType.ChannelEdit, () => new ChannelEditMessage() },
				{ (ushort)ClientMessageType.ClientAudioSourceStateChange, () => new ClientAudioSourceStateChangeMessage() },
			    { (ushort)ClientMessageType.Connect, () => new ConnectMessage() },
				{ (ushort)ClientMessageType.Disconnect, () => new DisconnectMessage() },
				{ (ushort)ClientMessageType.Join, () => new JoinMessage() },
				{ (ushort)ClientMessageType.KickUser, () => new KickUserMessage() },
			    { (ushort)ClientMessageType.Login, () => new LoginMessage() },
				{ (ushort)ClientMessageType.PunchThrough, () => new PunchThroughMessage() },
				{ (ushort)ClientMessageType.QueryServer, () => new QueryServerMessage() },
				{ (ushort)ClientMessageType.Register, () => new RegisterMessage() },
				{ (ushort)ClientMessageType.RegistrationApproval, () => new RegistrationApprovalMessage() },
			    { (ushort)ClientMessageType.RequestChannelList, () => new RequestChannelListMessage() },
			    { (ushort)ClientMessageType.RequestSource, () => new RequestSourceMessage() },
			    { (ushort)ClientMessageType.RequestSourceList, () => new RequestSourceListMessage() },
			    { (ushort)ClientMessageType.RequestUserList, () => new RequestUserListMessage() },
			    { (ushort)ClientMessageType.RequestMuteSource, () => new RequestMuteSourceMessage() },
				{ (ushort)ClientMessageType.RequestMuteUser, () => new RequestMuteUserMessage() },
				{ (ushort)ClientMessageType.SetComment, () => new SetCommentMessage() },
				{ (ushort)ClientMessageType.SetStatus, () => new SetStatusMessage() },
				{ (ushort)ClientMessageType.SetPermissions, () => new SetPermissionsMessage() },
				{ (ushort)ClientMessageType.Ping, () => new ClientPingMessage() },
			});

			//#else

			//Type msgb = typeof(MessageBase);
			//Dictionary<ushort, Func<MessageBase>> messageTypes = new Dictionary<ushort, Func<MessageBase>>();
			//foreach (Type t in Assembly.GetExecutingAssembly ().GetTypes ().Where (t => msgb.IsAssignableFrom (t) && !t.IsAbstract))
			//{
			//    var ctor = t.GetConstructor (Type.EmptyTypes);

			//    var dctor = new DynamicMethod (t.Name, msgb, null);
			//    var il = dctor.GetILGenerator ();

			//    il.Emit (OpCodes.Newobj, ctor);
			//    il.Emit (OpCodes.Ret);

			//    Func<MessageBase> dctord = (Func<MessageBase>)dctor.CreateDelegate (typeof (Func<MessageBase>));
			//    MessageBase dud = dctord();
			//    messageTypes.Add (dud.MessageTypeCode, dctord);
			//}

			//MessageTypes = new ReadOnlyDictionary<ushort, Func<MessageBase>> (messageTypes);

			//#endif
		}
	}
}