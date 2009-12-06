﻿// Copyright (c) 2009, Eric Maupin
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
using System.Text;

namespace Gablarski.Messages
{
	// Next: 39

	public enum ClientMessageType
		: ushort
	{
		Connect = 1,
		Login = 3,
		Join = 37,
		Disconnect = 5,
		QueryServer = 34,

		RequestSourceList = 15,
		RequestSource = 7,
		AudioData = 9,
		ClientAudioSourceStateChange = 28,
		RemoveSource = 26,

		RequestServerInfo = 11,
		RequestUserList = 13,
		
		RequestChannelList = 18,
		ChangeChannel = 20,
		EditChannel = 22,
		
		RequestMute = 30,
	}

	public enum ServerMessageType
		: ushort
	{
		ConnectionRejected = 2,
		QueryServerResult = 36,
		ServerInfoReceived = 12,
		LoginResult = 4,
		JoinResult = 38,
		//Disconnect = 6,
		PermissionDenied = 32,

		SourceListReceived = 16,
		SourceResult = 8,
		AudioDataReceived = 10,
		AudioSourceStateChange = 29,
		SourcesRemoved = 25,
	
		UserLoggedIn = 24,
		Permissions = 33,
		UserListReceived = 14,
		UserDisconnected = 17,
		UserChangedChannel = 27,

		ChannelListReceived = 19,
		ChangeChannelResult = 21,
		ChannelEditResult = 23,

		Muted = 31,
	}

	public abstract class Message<TMessage>
		: MessageBase
	{
		protected Message (TMessage messageType)
		{
			this.MessageType = messageType;
		}

		public TMessage MessageType
		{
			get;
			protected set;
		}
	}

	//public abstract class DualMessage
	//    : MessageBase
	//{
	//    protected DualMessage (ClientMessage clientMessageType, ServerMessage serverMessageType)
	//    {
	//        this.ClientMessageType = clientMessageType;
	//        this.ServerMessageType = serverMessageType;
	//    }

	//    public ClientMessage ClientMessageType
	//    {
	//        get;
	//        protected set;
	//    }

	//    public ServerMessage ServerMessageType
	//    {
	//        get;
	//        protected set;
	//    }
	//}
}