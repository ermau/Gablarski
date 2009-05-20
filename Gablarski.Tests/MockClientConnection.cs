using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gablarski.Messages;

namespace Gablarski.Tests
{
	public class MockClientConnection
		: IClientConnection
	{
		public MockClientConnection (MockServerConnection server)
		{
			this.server = server;
		}

		public void Receive (MessageBase message)
		{
			var received = this.MessageReceived;
			if (received != null)
				received (this, new MessageReceivedEventArgs (this, message));
		}

		#region IClientConnection Members

		public event EventHandler Connected;

		public void Connect (string host, int port)
		{
			throw new NotSupportedException ();
		}

		#endregion

		#region IConnection Members

		public event EventHandler<MessageReceivedEventArgs> MessageReceived;
		public event EventHandler<ConnectionEventArgs> Disconnected;

		public void Send (MessageBase message)
		{
			this.server.Receive (message);
		}

		public void Disconnect ()
		{
			var dced = this.Disconnected;
			if (dced != null)
				dced (this, new ConnectionEventArgs (this));
		}

		#endregion

		private readonly MockServerConnection server;
	}
}