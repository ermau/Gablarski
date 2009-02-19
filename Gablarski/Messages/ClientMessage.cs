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
		protected ClientMessage (ClientMessages messageType, IEndPoint endpoint)
			: base (messageType, endpoint)
		{
		}

		protected ClientMessage (ClientMessages messageType, IEndPoint endpoint, IValueReader payload)
			: base (messageType, endpoint, payload)
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