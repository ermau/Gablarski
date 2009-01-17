using System;
using Lidgren.Network;

namespace Gablarski
{
	public class ConnectionEventArgs
		: EventArgs
	{
		public ConnectionEventArgs (NetConnection connection, NetBuffer buffer)
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

	public class ReasonEventArgs
		: ConnectionEventArgs
	{
		public ReasonEventArgs (NetConnection connection, NetBuffer buffer, string reason)
			: base (connection, buffer)
		{
			this.Reason = reason;
		}

		public ReasonEventArgs (ConnectionEventArgs e, string reason)
			: this (e.Connection, e.Buffer, reason)
		{
		}

		public string Reason
		{
			get; private set;
		}
	}
}