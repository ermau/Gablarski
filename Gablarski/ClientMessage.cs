using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gablarski
{
	public class ClientMessage
		: Message<ClientMessages>
	{
		public override byte[] ToByteArray ()
		{
			byte[] buffer = new byte[5];
			buffer[0] = ClientMessage.FirstByte;
			Array.Copy (this.Connection.AuthHash.GetBytes (), buffer, 1);

			return buffer;
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