using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lidgren.Network;

namespace Gablarski.Server
{
	public class ServerState
	{
		public IEnumerable<NetConnection> Connections
		{
			get { return this.connections.Ones; }
		}

		public IEnumerable<IUser> Users
		{
			get { return this.users.Twos; }
		}

		public IUser this[NetConnection connection]
		{
			get
			{
				if (!this.users.Contains (connection))
					return null;

				return this.users[connection];
			}
		}

		public NetConnection this[int hash]
		{
			get { return this.connections[hash]; }
		}

		public NetConnection this[IUser user]
		{
			get { return this.users[user]; }
		}

		public bool Contains (NetConnection connection)
		{
			return this.connections.Contains (connection);
		}

		public bool Contains (int hash)
		{
			return this.connections.Contains (hash);
		}

		public void AddConnection (NetConnection connection, int hash)
		{
			connection.Tag = hash;
			this.connections[connection] = hash;
		}

		public void AddUser (NetConnection connection, IUser user)
		{
			this.users[connection] = user;
		}

		public void Remove (NetConnection connection)
		{
			this.connections.Remove (connection);
		}

		private PairCollection<NetConnection, IUser> users = new PairCollection<NetConnection, IUser> ();
		private PairCollection<NetConnection, int> connections = new PairCollection<NetConnection, int> ();
	}
}