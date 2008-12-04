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

		protected override uint MessageTypeCode
		{
			get { return (uint)this.MessageType; }
		}
	}

	public enum ClientMessages
		: uint
	{
		Ping		= 1,
		Login		= 2
	}
}