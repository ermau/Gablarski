using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gablarski.Messages;

namespace Gablarski.Tests
{
	public class MockServerConnection
		: IConnection
	{
		public MockServerConnection ()
		{
			this.client = new MockClientConnection (this);
		}

		public void Receive (MessageBase message)
		{
			var received = this.MessageReceived;
			if (received != null)
				received (this, new MessageReceivedEventArgs (this, message));
		}

		#region IConnection Members
		public event EventHandler<MessageReceivedEventArgs> MessageReceived;
		public event EventHandler<ConnectionEventArgs> Disconnected;

		public void Send (MessageBase message)
		{
			this.client.Receive (message);
		}

		public void Disconnect ()
		{
			var dced = this.Disconnected;
			if (dced != null)
				dced (this, new ConnectionEventArgs (this));
		}

		#endregion

		private readonly MockClientConnection client;
	}
}
