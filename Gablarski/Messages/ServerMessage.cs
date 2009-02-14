using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gablarski.Messages
{
	public abstract class ServerMessage
		: Message<ServerMessageType>
	{
		protected ServerMessage (ServerMessageType messageType, AuthedClient client)
			: base (messageType, client)
		{
			this.MessageType = messageType;
		}

		protected ServerMessage (ServerMessageType messageType, IEnumerable<AuthedClient> clients)
			: base (messageType, clients)
		{
			this.MessageType = messageType;
		}

		protected override ushort MessageTypeCode
		{
			get { return (ushort)this.MessageType; }
		}

		protected override bool SendAuthHash
		{
			get { return (this.MessageType == ServerMessageType.Connected); }
		}
	}

	public enum ServerMessageType
		: ushort
	{
		Pingback			= 1,
		Acknowledge			= 2,
		Connected			= 3,
		LoggedIn			= 4,
		UserConnected		= 5,
		UserDisconnected	= 6,
		MediaData			= 7,
		UserList			= 8,
		ChannelList			= 9,
		SourceCreated		= 10
	}
}