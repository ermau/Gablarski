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

		public UserConnection UserConnection
		{
			get { return (this.Connection.Tag as UserConnection); }
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

	public class ReasonEventArgs
		: ConnectionEventArgs
	{
		public ReasonEventArgs (NetConnection connection, NetBuffer buffer, string reason)
			: base (connection, buffer)
		{
			this.Reason = reason;
		}

		public string Reason
		{
			get; private set;
		}
	}
}