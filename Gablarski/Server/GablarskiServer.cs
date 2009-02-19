using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gablarski.Client;
using Gablarski.Messages;

namespace Gablarski.Server
{
	public class GablarskiServer
	{
		public GablarskiServer (IUserProvider userProvider)
		{
			this.userProvider = userProvider;
		}

		public IEnumerable<IConnectionProvider> ConnectionProviders
		{
			get
			{
				lock (connectionLock)
				{
					return this.availableConnections.ToArray ();
				}
			}

			set
			{
				this.availableConnections = value.ToList();
			}
		}

		public void AddConnectionProvider (IConnectionProvider connection)
		{
			connection.ConnectionMade += OnConnectionMade;
			connection.StartListening ();

			lock (connectionLock)
			{
				this.availableConnections.Add (connection);
			}
		}

		public void RemoveConnectionProvider (IConnectionProvider connection)
		{
			connection.StopListening ();
			connection.ConnectionMade -= this.OnConnectionMade;

			lock (connectionLock)
			{
				this.availableConnections.Remove (connection);
			}
		}

		public void Disconnect (IConnection connection, string reason)
		{
			if (connection == null)
				throw new ArgumentNullException ("connection");

			connection.Disconnect (reason);

			lock (connectionLock)
			{
				if (this.connections.Remove (connection))
					connection.MessageReceived -= this.OnMessageReceived;
			}
		}

		private readonly object connectionLock = new object();
		private List<IConnectionProvider> availableConnections = new List<IConnectionProvider> ();
		private readonly List<IConnection> connections = new List<IConnection>();
		private readonly IUserProvider userProvider;

		protected virtual void OnMessageReceived (object sender, MessageReceivedEventArgs e)
		{
			/* If a connection sends a garbage message or something invalid,
			 * the provider should 'send' a disconnected message. */
			var m = (e.Message as ClientMessage);
			if (m == null)
			{
				Disconnect (e.Connection, "Invalid message.");
				return;
			}
		}
		
		protected virtual void OnConnectionMade (object sender, ConnectionMadeEventArgs e)
		{
			e.Connection.MessageReceived += this.OnMessageReceived;

			lock (connectionLock)
			{
				this.connections.Add (e.Connection);
			}
		}
	}
}