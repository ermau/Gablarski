using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gablarski.Messages
{
	/// <summary>
	/// Client -> Server message
	/// </summary>
	public abstract class ClientMessage
		: Message<ClientMessages>
	{
		protected ClientMessage (ClientMessages messageType)
			: base (messageType)
		{
		}

		protected ClientMessage (ClientMessages messageType, IValueReader payload)
			: base (messageType, payload)
		{
		}

		protected override ushort MessageTypeCode
		{
			get { return (ushort)this.MessageType; }
		}
	}

	public enum ClientMessages
		: ushort
	{
		RequestToken	= 1,
		Login			= 2,
		Disconnect		= 3,
		AudioData		= 4
	}
}