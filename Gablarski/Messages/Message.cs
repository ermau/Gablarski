using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gablarski.Messages
{
	public enum ClientMessages
		: ushort
	{
		RequestToken = 1,
		Login = 3,
		Disconnect = 5,
		AudioData = 7
	}

	public enum ServerMessageType
		: ushort
	{
		Token = 2,
		LoginResult = 4,
		Disconnect = 6
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