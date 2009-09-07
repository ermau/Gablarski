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

namespace Gablarski.Messages
{
	public class QueryServerResultMessage
		: ServerMessage
	{
		public QueryServerResultMessage()
			: base (ServerMessageType.QueryServerResult)
		{
		}

		public IEnumerable<UserInfo> Users
		{
			get; set;
		}

		public IEnumerable<ChannelInfo> Channels
		{
			get; set;
		}

		public ServerInfo ServerInfo
		{
			get; set;
		}

		#region Overrides of MessageBase

		public override void WritePayload(IValueWriter writer)
		{
			writer.WriteInt32 (this.Users.Count());
			foreach (var u in this.Users)
				u.Serialize (writer);

			writer.WriteInt32 (this.Channels.Count());
			foreach (var c in this.Channels)
				c.Serialize (writer);

			this.ServerInfo.Serialize (writer);
		}

		public override void ReadPayload(IValueReader reader)
		{
			UserInfo[] users = new UserInfo[reader.ReadInt32()];
			for (int i = 0; i < users.Length; ++i)
				users[i] = new UserInfo(reader);
			
			this.Users = users;

			ChannelInfo[] channels = new ChannelInfo[reader.ReadInt32()];
			for (int i = 0; i < channels.Length; ++i)
				channels[i] = new ChannelInfo(reader);

			this.Channels = channels;

			this.ServerInfo = new ServerInfo (reader);
		}

		#endregion
	}
}