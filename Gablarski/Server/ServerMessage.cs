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
		public ServerMessage (ServerMessages messageType, UserConnection connection)
			: base (messageType, connection)
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
		Pingback		= 1,
		Acknowledge	= 2,
		Connected		= 3,
		LoggedIn		= 4
	}
}