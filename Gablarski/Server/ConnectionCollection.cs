using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gablarski.Messages;

namespace Gablarski.Server
{
	public class ConnectionCollection
	{
		public PlayerInfo this[IConnection key]
		{
			get
			{
				lock (lck)
				{
					return this.players[key];
				}
			}
		}

		public void Add (IConnection connection)
		{
			lock (lck)
			{
				connections.Add (connection);
			}
		}

		public void Add (IConnection connection, PlayerInfo player)
		{
			lock (lck)
			{
				if (!this.connections.Contains (connection))
					this.connections.Add (connection);

				this.players[connection] = player;
			}
		}

		public void Send (MessageBase message, Func<IConnection, bool> selector)
		{
			lock (lck)
			{
				foreach (IConnection connection in connections.Where (selector))
					connection.Send (message);
			}
		}

		private object lck = new object();
		private readonly List<IConnection> connections = new List<IConnection>();
		private readonly Dictionary<IConnection, PlayerInfo> players = new Dictionary<IConnection, PlayerInfo>();
	}
}