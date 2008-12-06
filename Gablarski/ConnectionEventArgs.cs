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
		public ConnectionEventArgs (UserConnection connection, NetBuffer buffer)
		{
			this.UserConnection = connection;
			this.Buffer = buffer;
		}

		public UserConnection UserConnection
		{
			get;
			private set;
		}

		public NetConnection Connection
		{
			get { return this.UserConnection.Connection; }
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
		public ReasonEventArgs (UserConnection connection, NetBuffer buffer, string reason)
			: base (connection, buffer)
		{
			this.Reason = reason;
		}

		public ReasonEventArgs (ConnectionEventArgs e, string reason)
			: this (e.UserConnection, e.Buffer, reason)
		{
		}

		public string Reason
		{
			get; private set;
		}
	}
}