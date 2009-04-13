using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gablarski.Messages
{
	public enum ClientMessageType
		: ushort
	{
		RequestToken = 1,
		Login = 3,
		Disconnect = 5,
		RequestSource = 7,
	}

	public enum ServerMessageType
		: ushort
	{
		TokenResult = 2,
		LoginResult = 4,
		Disconnect = 6,
		SourceResult = 8
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
}