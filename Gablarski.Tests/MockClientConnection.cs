using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gablarski.Messages;
using System.Threading;
using System.Net;

namespace Gablarski.Tests
{
	public class MockClientConnection
		: IClientConnection
	{
		public MockClientConnection (MockServerConnection server)
		{
			this.connected = true;
			this.server = server;
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

		public MessageBase DequeueMessage()
		{
			while (this.messages.Count == 0)
				Thread.Sleep (1);

			lock (this.messages)
			{
				return this.messages.Dequeue();
			}
		}

		#region IClientConnection Members

		public event EventHandler Connected;

		public void Connect (string host, int port)
		{
			throw new NotSupportedException ();
		}

		public void Connect (IPEndPoint endpoint)
		{
			throw new NotSupportedException ();
		}

		#endregion

		#region IConnection Members

		public event EventHandler<MessageReceivedEventArgs> MessageReceived;
		public event EventHandler<ConnectionEventArgs> Disconnected;

		public bool IsConnected
		{
			get { return this.connected; }
		}

		public void Send (MessageBase message)
		{
			this.server.Receive (message);
		}

		public void Disconnect ()
		{
			this.connected = false;
			var dced = this.Disconnected;
			if (dced != null)
				dced (this, new ConnectionEventArgs (this));
		}

		#endregion

		private readonly MockServerConnection server;
		private bool connected;

        private Queue<MessageBase> messages = new Queue<MessageBase>();
	}
}