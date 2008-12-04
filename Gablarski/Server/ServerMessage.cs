using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gablarski.Server
{
	/// <summary>
	/// Server -> Client message.
	/// </summary>
	public class ServerMessage
		: Message<ServerMessages>
	{
		public ServerMessage (UserConnection connection)
			: base (connection)
		{
		}

		protected override uint MessageTypeCode
		{
			get { return (uint)this.MessageType; }
		}
	}

	public enum ServerMessages
		: uint
	{
		Pingback		= 1,
		Acknowledge		= 2,
		Connected		= 3,
		Disconnected	= 4,
		UserNotFound	= 5,
		LoginRejected	= 6,
		ChannelList		= 7,
		PlayerList		= 8
	}
}