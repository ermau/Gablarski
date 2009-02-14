using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gablarski.Client;
using Gablarski.Messages;
using Mono.Rocks;

namespace Gablarski.Server
{
	public class GablarskiServer
	{
		public GablarskiServer (IUserProvider userProvider)
		{
			this.userProvider = userProvider;
		}

		public IEnumerable<IAvailableConnection> Connections
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

		public void AddAvailableConnection (IAvailableConnection connection)
		{
			connection.ConnectionMade += OnConnectionMade;
			connection.StartListening ();

			lock (connectionLock)
			{
				this.availableConnections.Add (connection);
			}
		}

		public void RemoveAvailableConnection (IAvailableConnection connection)
		{
			connection.StopListening ();
			connection.ConnectionMade -= this.OnConnectionMade;

			lock (connectionLock)
			{
				this.availableConnections.Remove (connection);
			}
		}

		public void Disconnect (IConnection connection)
		{
			if (connection == null)
				throw new ArgumentNullException ("connection");

			lock (connectionLock)
			{
				if (this.connections.Remove (connection))
					connection.MessageReceived -= this.OnMessageReceived;
			}
		}

		private readonly object connectionLock = new object();
		private List<IAvailableConnection> availableConnections = new List<IAvailableConnection> ();
		private readonly List<IConnection> connections = new List<IConnection>();
		private readonly IUserProvider userProvider;

		protected virtual void OnMessageReceived (object sender, MessageReceivedEventArgs e)
		{
			/* If a connection sends a garbage message or something invalid,
			 * the provider should 'send' a disconnected message. */
			var m = (e.Message as ClientMessage);
			if (m == null || m.MessageType == ClientMessages.Disconnect)
			{
				Disconnect (e.Connection);
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