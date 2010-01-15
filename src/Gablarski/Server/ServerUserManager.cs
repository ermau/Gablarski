// Copyright (c) 2009, Eric Maupin
// All rights reserved.
//
// Redistribution and use in source and binary forms, with
// or without modification, are permitted provided that
// the following conditions are met:
//
// - Redistributions of source code must retain the above 
//   copyright notice, this list of conditions and the
//   following disclaimer.
//
// - Redistributions in binary form must reproduce the above
//   copyright notice, this list of conditions and the
//   following disclaimer in the documentation and/or other
//   materials provided with the distribution.
//
// - Neither the name of Gablarski nor the names of its
//   contributors may be used to endorse or promote products
//   or services derived from this software without specific
//   prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS
// AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED
// WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
// WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR
// PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT
// HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT,
// INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
// (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE
// GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
// INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY,
// WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
// NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
// THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH
// DAMAGE.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cadenza.Collections;
using Gablarski.Messages;

namespace Gablarski.Server
{
	public class ServerUserManager
		: IServerUserManager
	{
		public UserInfo	this [int userId]
		{
			get
			{
				UserInfo user;

				lock (SyncRoot)
				{
					user = connectedUsers.Keys.FirstOrDefault (u => u.UserId == userId);
				}

				return user;
			}
		}

		#region IConnectionManager Members

		public void Connect (IConnection connection)
		{
			if (connection == null)
				throw new ArgumentNullException ("connection");

			lock (SyncRoot)
			{
				connections.Add (connection);
			}
		}

		public bool GetIsConnected (IConnection connection)
		{
			if (connection == null)
				throw new ArgumentNullException ("connection");

			lock (SyncRoot)
			{
				return connections.Contains (connection);
			}
		}

		public bool GetIsConnected (UserInfo user)
		{
			if (user == null)
				throw new ArgumentNullException ("user");

			lock (SyncRoot)
			{
				var c = GetConnection (user);
				if (c != null)
					return connections.Contains (c);
			}

			return false;
		}

		public void Join (IConnection connection, UserInfo user)
		{
			if (connection == null)
				throw new ArgumentNullException ("connection");
			if (user == null)
				throw new ArgumentNullException ("user");

			lock (SyncRoot)
			{
				if (!connections.Contains (connection) || joined.Contains (connection))
					return;

				connectedUsers[user] = connection;
				joined.Add (connection);
			}
		}

		public bool GetIsJoined (IConnection connection)
		{
			if (connection == null)
				throw new ArgumentNullException ("connection");

			lock (SyncRoot)
			{
				return joined.Contains (connection);
			}
		}

		public bool GetIsJoined (UserInfo user)
		{
			if (user == null)
				throw new ArgumentNullException ("user");

			lock (SyncRoot)
			{
				var c = GetConnection (user);
				if (c != null)
					return joined.Contains (c);
			}

			return false;
		}

		public void Login (IConnection connection, UserInfo user)
		{
			if (connection == null)
				throw new ArgumentNullException ("connection");
			if (user == null)
				throw new ArgumentNullException ("user");

			lock (SyncRoot)
			{
				if (!connections.Contains (connection))
					return;

				connectedUsers.Add (user, connection);
				loggedIn.Add (connection);
			}
		}

		public bool GetIsLoggedIn (IConnection connection)
		{
			if (connection == null)
				throw new ArgumentNullException ("connection");

			lock (SyncRoot)
			{
				return loggedIn.Contains (connection);
			}
		}

		public bool GetIsLoggedIn (UserInfo user)
		{
			if (user == null)
				throw new ArgumentNullException ("user");

			lock (SyncRoot)
			{
				var c = GetConnection (user);
				if (c != null)
					return loggedIn.Contains (c);
			}

			return false;
		}

		public void Disconnect (IConnection connection)
		{
			if (connection == null)
				throw new ArgumentNullException ("connection");

			lock (SyncRoot)
			{
				if (!connections.Remove (connection))
					return;
				
				loggedIn.Remove (connection);
				connectedUsers.Inverse.Remove (connection);
			}
		}

		public void Associate (IConnection connection, UserInfo user)
		{
			if (connection == null)
				throw new ArgumentNullException ("connection");
			if (user == null)
				throw new ArgumentNullException ("user");

			lock (SyncRoot)
			{
				connectedUsers.Add (user, connection);
			}
		}

		public void Send (MessageBase message, Func<IConnection, bool> predicate)
		{
			if (message == null)
				throw new ArgumentNullException ("message");
			if (predicate == null)
				throw new ArgumentNullException ("predicate");

			lock (SyncRoot)
			{
				foreach (var c in connections.Where (predicate))
					c.Send (message);
			}
		}

		public void Send (MessageBase message, Func<IConnection, UserInfo, bool> predicate)
		{
			if (message == null)
				throw new ArgumentNullException ("message");
			if (predicate == null)
				throw new ArgumentNullException ("predicate");

			lock (SyncRoot)
			{
				foreach (var kvp in connectedUsers.Where (kvp => predicate (kvp.Value, kvp.Key)))
					kvp.Value.Send (message);
			}
		}

		public UserInfo GetUser (IConnection connection)
		{
			if (connection == null)
				throw new ArgumentNullException ("connection");

			UserInfo user;// = null;
			lock (SyncRoot)
			{
				connectedUsers.TryGetKey (connection, out user);
				//if (connectedUsers.ContainsValue (connection))
				//    user = connectedUsers.FirstOrDefault (kvp => kvp.Value == connection).Key;
			}

			return user;
		}

		public IConnection GetConnection (UserInfo user)
		{
			if (user == null)
				throw new ArgumentNullException ("user");

			IConnection connection;
			lock (SyncRoot)
			{
				connectedUsers.TryGetValue (user, out connection);
			}

			return connection;
		}

		#endregion

		protected readonly object SyncRoot = new object();
		private readonly HashSet<IConnection> joined = new HashSet<IConnection>();
		private readonly HashSet<IConnection> loggedIn = new HashSet<IConnection>();
		private readonly HashSet<IConnection> connections = new HashSet<IConnection>();
		private readonly BidirectionalDictionary<UserInfo, IConnection> connectedUsers = new BidirectionalDictionary<UserInfo, IConnection>();

		#region IEnumerable Members

		public IEnumerator<UserInfo> GetEnumerator()
		{
			IEnumerable<UserInfo> e;
			lock (SyncRoot)
			{
				e = connectedUsers.Keys.ToList();
			}

			return e.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		#endregion
	}
}