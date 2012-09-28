using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Tempest;

namespace Gablarski.Tests
{
	public class ConnectionBuffer
		: IConnection
	{
		private readonly IConnection connection;

		public ConnectionBuffer (IConnection connection)
		{
			this.connection = connection;
			this.connection.MessageReceived += OnMessageReceived;
		}

		public void Dispose ()
		{
			this.connection.Dispose();
		}

		public bool IsConnected
		{
			get { return this.connection.IsConnected; }
		}

		public IEnumerable<Protocol> Protocols
		{
			get { return this.connection.Protocols; }
		}

		public MessagingModes Modes
		{
			get { return this.connection.Modes; }
		}

		public EndPoint RemoteEndPoint
		{
			get { return this.connection.RemoteEndPoint; }
		}

		public IAsymmetricKey RemoteKey
		{
			get { return this.connection.RemoteKey; }
		}

		public int ResponseTime
		{
			get { return this.connection.ResponseTime; }
		}

		public event EventHandler<MessageEventArgs> MessageReceived;
		public event EventHandler<MessageEventArgs> MessageSent;
		public event EventHandler<DisconnectedEventArgs> Disconnected;

		public void Send (Message message)
		{
			this.connection.Send (message);
		}

		public Task<TResponse> SendFor<TResponse> (Message message, int timeout = 0) where TResponse : Message
		{
			return this.connection.SendFor<TResponse> (message, timeout);
		}

		public void SendResponse (Message originalMessage, Message response)
		{
			this.connection.SendResponse (originalMessage, response);
		}

		public IEnumerable<MessageEventArgs> Tick ()
		{
			return this.connection.Tick();
		}

		public void Disconnect ()
		{
			this.connection.Disconnect();
		}

		public void Disconnect (ConnectionResult reason, string customReason = null)
		{
			this.connection.Disconnect (reason, customReason);
		}

		public void DisconnectAsync ()
		{
			this.connection.DisconnectAsync();
		}

		public void DisconnectAsync (ConnectionResult reason, string customReason = null)
		{
			this.connection.DisconnectAsync (reason, customReason);
		}

		public T DequeueAndAssertMessage<T>()
			where T : Message
		{
			Message msg = DequeueMessage();

			if (!(msg is T))
				Assert.Fail ("Message was " + msg.GetType().Name + ", not expected " + typeof (T).Name);

			return (T)msg;
		}

		public Message DequeueMessage (bool wait = true)
		{
			ushort tick = 0;

			Message msg;
			while (!this.messages.TryDequeue (out msg) && tick++ < (UInt16.MaxValue - 1))
				Thread.Sleep (1);

			if (tick == UInt16.MaxValue)
				Assert.Fail ("Message never arrived");

			return msg;
		}

		public void AssertNoMessage()
		{
			Assert.IsTrue (this.messages.Count == 0);
		}

		private readonly ConcurrentQueue<Message> messages = new ConcurrentQueue<Message>();
		private void OnMessageReceived (object sender, MessageEventArgs e)
		{
			this.messages.Enqueue (e.Message);
		}
	}
}