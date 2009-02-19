using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gablarski.Messages;

namespace Gablarski
{
	public interface IConnection
	{
		event EventHandler<MessageReceivedEventArgs> MessageReceived;

		void Send (MessageBase message);

		void Disconnect (string reason);
	}

	public class MessageReceivedEventArgs
		: EventArgs
	{
		public MessageReceivedEventArgs (IConnection connection, MessageBase message)
		{
			this.Connection = connection;
			this.Message = message;
		}

		public IConnection Connection
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets the message that was received.
		/// </summary>
		public MessageBase Message
		{
			get;
			private set;
		}
	}
}