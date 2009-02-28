using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gablarski.Messages
{
	public abstract class ServerMessage
		: Message<ServerMessageType>
	{
		protected ServerMessage (ServerMessageType messageType)
			: base (messageType)
		{
			this.MessageType = messageType;
		}

		public override ushort MessageTypeCode
		{
			get { return (ushort)this.MessageType; }
		}
	}

	public enum ServerMessageType
		: ushort
	{
		Token = 1
	}
}