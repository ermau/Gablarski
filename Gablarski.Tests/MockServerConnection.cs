using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gablarski.Messages;
using System.Threading;

namespace Gablarski.Tests
{
	public class MockServerConnection
		: IConnection
	{
		public MockServerConnection ()
		{
			this.connected = true;
			this.client = new MockClientConnection (this);
		}

		public MockClientConnection Client
		{
			get { return this.client; }
		}

		public void Receive (MessageBase message)
		{
			lock (this.messages)
			{
				this.messages.Enqueue (message);
			}

			var received = this.MessageReceived;
			if (received != null)
				received (this, new MessageReceivedEventArgs (this, message));
		}

		public MessageBase DequeueMessage ()
		{
			while (this.messages.Count == 0)
				Thread.Sleep (1);

			lock (this.messages)
			{
				return this.messages.Dequeue ();
			}
		}

		#region IConnection Members
		public event EventHandler<MessageReceivedEventArgs> MessageReceived;
		public event EventHandler<ConnectionEventArgs> Disconnected;

		public bool IsConnected
		{
			get { return this.connected; }
		}

		public void Send (MessageBase message)
		{
			this.client.Receive (message);
		}

		public void Disconnect ()
		{
			this.connected = false;
			var dced = this.Disconnected;
			if (dced != null)
				dced (this, new ConnectionEventArgs (this));
		}

		#endregion

		private bool connected;
		private readonly MockClientConnection client;

        private Queue<MessageBase> messages = new Queue<MessageBase>();
	}
}
