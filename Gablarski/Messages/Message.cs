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
		Disconnect = 4,
		AudioData = 5
	}

	public enum ServerMessageType
		: ushort
	{
		Token = 2
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