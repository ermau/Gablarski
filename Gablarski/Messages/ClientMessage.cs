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

		public override ushort MessageTypeCode
		{
			get { return (ushort)this.MessageType; }
		}
	}
}