using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Gablarski.Server;
using Gablarski.Messages;

namespace Gablarski.Tests
{
	public class MockConnectionProvider
		: IConnectionProvider
	{
		public MockServerConnection EstablishConnection ()
		{
			var connection = new MockServerConnection ();

			var connectionMade = this.ConnectionMade;
			if (connectionMade != null)
				connectionMade (this, new ConnectionEventArgs (connection));

			return connection;
		}

		public bool IsListening
		{
			get;
			private set;
		}

		#region IConnectionProvider Members

		public event EventHandler<ConnectionEventArgs> ConnectionMade;

		public event EventHandler<ConnectionlessMessageReceivedEventArgs> ConnectionlessMessageReceived;

		/// <summary>
		/// Sends a connectionless <paramref name="message"/> to the <paramref name="endpoint"/>.
		/// </summary>
		/// <param name="message">The message to send.</param>
		/// <param name="endpoint">The endpoint to send the <paramref name="message"/> to.</param>
		/// <exception cref="ArgumentNullException"><paramref name="message"/> is <c>null</c>.</exception>
		/// <exception cref="ArgumentNullException"><paramref name="endpoint"/> is <c>null</c>.</exception>
		/// <exception cref="ArgumentException"><paramref name="message"/> is set as a reliable message.</exception>
		/// <seealso cref="IConnectionProvider.ConnectionlessMessageReceived"/>
		public void SendConnectionlessMessage (MessageBase message, EndPoint endpoint)
		{
		}

		public void StartListening (IServerContext serverContext)
		{
			this.IsListening = true;
		}

		public void StopListening ()
		{
			this.IsListening = false;
		}

		#endregion
	}
}