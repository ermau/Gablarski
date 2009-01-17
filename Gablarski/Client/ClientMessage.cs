using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lidgren.Network;

namespace Gablarski.Client
{
	/// <summary>
	/// Client -> Server message
	/// </summary>
	public class ClientMessage
		: Message<ClientMessages>
	{
		public ClientMessage (ClientMessages messageType, NetConnection connection)
			: base (messageType, connection)
		{
		}

		protected override uint MessageTypeCode
		{
			get { return (uint)this.MessageType; }
		}

		protected override bool SendAuthHash
		{
			get { return true; }
		}
	}

	public enum ClientMessages
		: uint
	{
		Ping			= 1,
		Login			= 2,
		Disconnect		= 3,
		AudioData		= 4
	}
}