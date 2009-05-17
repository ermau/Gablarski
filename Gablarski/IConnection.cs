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
		event EventHandler<ConnectionEventArgs> Disconnected;

		void Send (MessageBase message);

		void Disconnect ();
	}

	public static class ConnectionExtensions
	{
		public static void Send (this IEnumerable<IConnection> connections, MessageBase message)
		{
			foreach (var connection in connections)
				connection.Send (message);
		}

		public static void Send (this IEnumerable<IConnection> connections, MessageBase message, Func<IConnection, bool> predicate)
		{
			foreach (var connection in connections)
			{
				if (predicate (connection))
					connection.Send (message);
			}
		}
	}

	public class MessageReceivedEventArgs
		: ConnectionEventArgs
	{
		public MessageReceivedEventArgs (IConnection connection, MessageBase message)
			: base (connection)
		{
			this.Message = message;
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