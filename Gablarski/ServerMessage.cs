using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gablarski
{
	public class ServerMessage
		: Message<ServerMessages>
	{
		public override byte[] ToByteArray ()
		{
			byte[] buffer = new byte[5];
			buffer[0] = FirstByte;
			Array.Copy (this.Connection.AuthHash.GetBytes (), buffer, 1);

			return buffer;
		}
	}

	public enum ServerMessages
		: uint
	{
		Pingback		= 1,
		Connected		= 2,
		Disconnected	= 3,
		UserNotFound	= 4,
		LoginRejected	= 5,
		ChannelList		= 6,
		PlayerList		= 7
	}
}