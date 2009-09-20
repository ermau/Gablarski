// Copyright (c) 2009, Eric Maupin
// All rights reserved.

// Redistribution and use in source and binary forms, with
// or without modification, are permitted provided that
// the following conditions are met:

// - Redistributions of source code must retain the above 
//   copyright notice, this list of conditions and the
//   following disclaimer.

// - Redistributions in binary form must reproduce the above
//   copyright notice, this list of conditions and the
//   following disclaimer in the documentation and/or other
//   materials provided with the distribution.

// - Neither the name of Gablarski nor the names of its
//   contributors may be used to endorse or promote products
//   derived from this software without specific prior
//   written permission.

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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gablarski.Messages;

namespace Gablarski.Server
{
	internal class ConnectionCollection
	{
		public int UserCount
		{
			get { return this.users.Count; }
		}

		public int ConnectionCount
		{
			get { return this.connections.Count; }
		}

		public ServerUserInfo this[IConnection key]
		{
			get
			{
				lock (lck)
				{
					if (this.users.ContainsKey (key))
						return this.users[key];
				}

				return null;
			}
		}

		public IConnection this[ServerUserInfo key]
		{
			get
			{
				lock (lck)
				{
					return this.users.FirstOrDefault (kvp => kvp.Value == key).Key;
				}
			}
		}

		public IConnection this[UserInfo key]
		{
			get
			{
				lock(lck)
				{
					return this.users.FirstOrDefault (kvp => kvp.Value == key).Key;
				}
			}
		}

		public KeyValuePair<IConnection, ServerUserInfo> this[int index]
		{
			get
			{
				lock (lck)
				{
					IConnection c = this.connections[index];
					ServerUserInfo p;
					this.users.TryGetValue (c, out p);

					return new KeyValuePair<IConnection, ServerUserInfo> (c, p);
				}
			}
		}

		public IEnumerable<UserInfo> Users
		{
			get
			{
				lock (lck)
				{
					ServerUserInfo[] copiedPlayers = new ServerUserInfo[this.users.Count];
					this.users.Values.CopyTo (copiedPlayers, 0);

					return copiedPlayers;
				}
			}
		}

		public bool NicknameInUse (string nickname, IConnection connection)
		{
			lock (lck)
			{
				return this.users.Any (kvp => kvp.Value.Nickname == nickname && kvp.Key != connection);
			}
		}

		public void Add (IConnection connection)
		{
			lock (lck)
			{
				connections.Add (connection);
			}
		}


		public void Add (IConnection connection, ServerUserInfo user)
		{
			lock (lck)
			{
				if (!this.connections.Contains (connection))
					this.connections.Add (connection);

				this.users[connection] = user;
			}
		}

		public UserInfo GetUser (string target)
		{
			lock (lck)
			{
				return this.users.FirstOrDefault (u => u.Value.Username == target).Value;
			}
		}

		public IConnection GetConnection (int userId)
		{
			lock (lck)
			{
				return (from kvp in this.users where kvp.Value.UserId.Equals (userId) select kvp.Key).FirstOrDefault();
			}
		}

		public ServerUserInfo GetUser (int userId)
		{
			lock (lck)
			{
				return this.users.Values.FirstOrDefault (u => u.UserId == userId);
			}
		}

		public bool UpdateIfExists (IConnection connection, ServerUserInfo user)
		{
			lock (lck)
			{
				if (!this.connections.Contains (connection))
					return false;

				this.users[connection] = user;
				return true;
			}
		}

		public bool UpdateIfExists (ServerUserInfo user)
		{
			lock (lck)
			{
				var old = this.users.FirstOrDefault (kvp => kvp.Value.UserId.Equals (user.UserId));
				if (old.Equals (default(KeyValuePair<IConnection, ServerUserInfo>)))
					return false;

				this.users[old.Key] = user;
				return true;
			}
		}

		public bool Remove (IConnection connection)
		{
			lock (lck)
			{
				this.users.Remove (connection);
				return this.connections.Remove (connection);
			}
		}

		public bool Remove (IConnection connection, out int userId)
		{
			userId = 0;

			lock (lck)
			{
				this.connections.Remove (connection);
				if (!this.users.ContainsKey (connection))
					return false;

				var info = this.users[connection];
				userId = info.UserId;
				this.users.Remove (connection);

				return true;
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

		public void Send (MessageBase message, Func<ServerUserInfo, bool> selector)
		{
			lock (lck)
			{
				foreach (var kvp in this.users)
				{
					if (!selector (kvp.Value))
						continue;

					kvp.Key.Send (message);
				}
			}
		}

		public void Send (MessageBase message, Func<IConnection, ServerUserInfo, bool> selector)
		{
			lock (lck)
			{
				foreach (var kvp in this.users)
				{
					if (!selector (kvp.Key, kvp.Value))
						continue;

					kvp.Key.Send (message);
				}
			}
		}

		private readonly object lck = new object();
		private readonly List<IConnection> connections = new List<IConnection>();
		private readonly Dictionary<IConnection, ServerUserInfo> users = new Dictionary<IConnection, ServerUserInfo>();
	}
}