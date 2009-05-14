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

		public IEnumerable<PlayerInfo> Players
		{
			get
			{
				lock (lck)
				{
					PlayerInfo[] copiedPlayers = new PlayerInfo[this.players.Count];
					this.players.Values.CopyTo (copiedPlayers, 0);

					return copiedPlayers;
				}
			}
		}

		/// <summary>
		/// Gets the player Id for the connection, 0 if the connection wasn't found.
		/// </summary>
		public long GetPlayerId (IConnection connection)
		{
			long id = 0;
			
			lock (lck)
			{
				if (this.players.ContainsKey (connection))
					id = this.players[connection].PlayerId;
			}

			return id;
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

		public void Send (MessageBase message)
		{
			lock (lck)
			{
				foreach (IConnection c in this.connections)
					c.Send (message);
			}
		}

		public void Send (MessageBase message, Func<IConnection, bool> selector)
		{
			lock (lck)
			{
				foreach (IConnection c in this.connections.Where (selector))
					c.Send (message);
			}
		}

		public void Send (MessageBase message, Func<PlayerInfo, bool> selector)
		{
			lock (lck)
			{
				foreach (var kvp in this.players)
				{
					if (!selector (kvp.Value))
						continue;

					kvp.Key.Send (message);
				}
			}
		}

		private object lck = new object();
		private readonly List<IConnection> connections = new List<IConnection>();
		private readonly Dictionary<IConnection, PlayerInfo> players = new Dictionary<IConnection, PlayerInfo>();
	}
}