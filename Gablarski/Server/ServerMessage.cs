using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gablarski.Server
{
	/// <summary>
	/// Server -> Client message.
	/// </summary>
	public class ServerMessage
		: Message<ServerMessages>
	{
		public ServerMessage (ServerMessages messageType)
			: base (messageType)
		{
		}

		public ServerMessage (ServerMessages messageType, UserConnection connection)
			: base (messageType, connection)
		{
		}

		public ServerMessage (ServerMessages messageType, IEnumerable<UserConnection> userConnections)
			: base (messageType, userConnections)
		{
		}

		protected override uint MessageTypeCode
		{
			get { return (uint)this.MessageType; }
		}

		protected override bool SendAuthHash
		{
			get { return (this.MessageType == ServerMessages.Connected); }
		}
	}

	public enum ServerMessages
		: uint
	{
		Pingback			= 1,
		Acknowledge			= 2,
		Connected			= 3,
		LoggedIn			= 4,
		UserConnected		= 5,
		UserDisconnected	= 6,
		VoiceData			= 7,
		UserList			= 8,
		ChannelList			= 9
	}
}