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
		public ClientMessage (UserConnection connection)
			: base (connection)
		{
		}

		public override void Encode (NetBuffer buffer)
		{
			EncodeHeader (buffer);
		}
	}

	public enum ClientMessages
		: uint
	{
		Ping		= 1,
		Connect		= 2,
		Login		= 3
	}
}