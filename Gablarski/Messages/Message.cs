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
		MediaData = 9,
		RequestServerInfo = 11,
		RequestPlayerList = 13,
	}

	public enum ServerMessageType
		: ushort
	{
		TokenResult = 2,
		LoginResult = 4,
		Disconnect = 6,
		SourceResult = 8,
		MediaDataReceived = 10,
		ServerInfoReceived = 12,
		PlayerListReceived = 14
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