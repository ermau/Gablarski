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
		protected ClientMessage (ClientMessages messageType, IConnection recipient)
			: base (messageType, recipient)
		{
		}

		protected ClientMessage (ClientMessages messageType, IConnection recipient, IValueReader payload)
			: base (messageType, recipient, payload)
		{
		}

		protected override ushort MessageTypeCode
		{
			get { return (ushort)this.MessageType; }
		}

		protected override bool SendAuthHash
		{
			get { return true; }
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