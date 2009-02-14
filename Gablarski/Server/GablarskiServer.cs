using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gablarski.Client;

namespace Gablarski.Server
{
	public class GablarskiServer
	{
		public IEnumerable<IAvailableConnection> Connections
		{
			get
			{
				lock (connectionLock)
				{
					return this.connections.ToArray ();
				}
			}

			set
			{
				this.connections = value.ToList();
			}
		}

		public void AddConnection (IAvailableConnection connection)
		{
			connection.MessageReceived += this.OnMessageReceived;
			connection.StartListening ();

			lock (connectionLock)
			{
				this.connections.Add (connection);
			}
		}

		public void RemoveConnection (IAvailableConnection connection)
		{
			connection.StopListening ();
			connection.MessageReceived -= this.OnMessageReceived;

			lock (connectionLock)
			{
				this.connections.Remove (connection);
			}
		}

		private object connectionLock = new object();
		private List<IAvailableConnection> connections = new List<IAvailableConnection> ();

		private void OnMessageReceived (object sender, MessageReceivedEventArgs e)
		{
			throw new NotImplementedException ();
		}
	}
}