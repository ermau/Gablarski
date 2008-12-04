using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lidgren.Network;

namespace Gablarski
{
	public class ConnectionEventArgs
		: EventArgs
	{
		public ConnectionEventArgs(NetConnection connection, NetBuffer buffer)
		{
			this.Connection = connection;
			this.Buffer = buffer;
		}

		public NetConnection Connection
		{
			get;
			private set;
		}

		public NetBuffer Buffer
		{
			get;
			private set;
		}
	}
}