// Copyright (c) 2011, Eric Maupin
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
using Cadenza.Collections;
using Tempest;

namespace Gablarski.Server
{
	public class ServerUserManager
		: IServerUserManager
	{
		public IUserInfo this [int userId]
		{
			get
			{
				return this.connectedUsers.Keys.FirstOrDefault (u => u.UserId == userId);
			}
		}

		#region IConnectionManager Members

		public IEnumerable<IConnection> GetConnections()
		{
			return connections.ToList();
		}

		public void Connect (IConnection connection)
		{
			if (connection == null)
				throw new ArgumentNullException ("connection");

			connections.Add (connection);
		}

		public bool GetIsConnected (IConnection connection)
		{
			if (connection == null)
				throw new ArgumentNullException ("connection");

			return connections.Contains (connection);
		}

		public bool GetIsConnected (IUserInfo user)
		{
			if (user == null)
				throw new ArgumentNullException ("user");

			var c = GetConnection (user);
			if (c != null)
				return connections.Contains (c);

			return false;
		}

		public void Join (IConnection connection, IUserInfo user)
		{
			if (connection == null)
				throw new ArgumentNullException ("connection");
			if (user == null)
				throw new ArgumentNullException ("user");

			if (!connections.Contains (connection) || joined.Contains (connection))
				return;

			connectedUsers.Inverse[connection] = new UserInfo (user);
			joined.Add (connection);
		}

		public bool GetIsJoined (IConnection connection)
		{
			if (connection == null)
				throw new ArgumentNullException ("connection");

			return joined.Contains (connection);
		}

		public bool GetIsJoined (IUserInfo user)
		{
			if (user == null)
				throw new ArgumentNullException ("user");

			var c = GetConnection (user);
			if (c != null)
				return joined.Contains (c);

			return false;
		}

		public void Move (IUserInfo user, IChannelInfo channel)
		{
			if (user == null)
				throw new ArgumentNullException ("user");
			if (channel == null)
				throw new ArgumentNullException ("channel");

			IConnection connection = GetConnection (user);
			if (connection == null)
				return;

			IUserInfo old;
			if (!connectedUsers.TryGetKey (connection, out old))
				return;

			UserInfo newUser = new UserInfo (old, channel.ChannelId);
			connectedUsers.Inverse[connection] = newUser;
		}

		public bool ToggleMute (IUserInfo user)
		{
			if (user == null)
				throw new ArgumentNullException ("user");

			IConnection connection = GetConnection (user);
			if (connection == null)
				return false;

			IUserInfo old;
			if (!connectedUsers.TryGetKey (connection, out old))
				return false;

			UserInfo newUser = new UserInfo (old, !old.IsMuted);
			connectedUsers.Inverse[connection] = newUser;

			return newUser.IsMuted;
		}

		public IUserInfo SetStatus (IUserInfo user, UserStatus newStatus)
		{
			if (user == null)
				throw new ArgumentNullException ("user");

			IConnection connection = GetConnection (user);
			if (connection == null)
				return null;

			IUserInfo old;
			if (!connectedUsers.TryGetKey (connection, out old))
				return null;

			UserInfo newUser = new UserInfo (old, newStatus);
			connectedUsers.Inverse[connection] = newUser;

			return newUser;
		}

		public IUserInfo SetComment (IUserInfo user, string comment)
		{
			if (user == null)
				throw new ArgumentNullException ("user");

			IConnection connection = GetConnection (user);
			if (connection == null)
				return null;

			IUserInfo old;
			if (!connectedUsers.TryGetKey (connection, out old))
				return null;

			UserInfo newUser = new UserInfo (old, comment);
			connectedUsers.Inverse[connection] = newUser;

			return newUser;
		}

		public void Login (IConnection connection, IUserInfo user)
		{
			if (connection == null)
				throw new ArgumentNullException ("connection");
			if (user == null)
				throw new ArgumentNullException ("user");
			
			if (!connections.Contains (connection))
				return;

			IConnection oldConnection;
			if (connectedUsers.TryGetValue (user, out oldConnection))
			{
				oldConnection.Disconnect();
				connectedUsers.Remove (user);
				loggedIn.Remove (oldConnection);
				connections.Remove (oldConnection);
			}

			connectedUsers.Add (user, connection);
			loggedIn.Add (connection);
		}

		public bool GetIsLoggedIn (IConnection connection)
		{
			if (connection == null)
				throw new ArgumentNullException ("connection");

			return loggedIn.Contains (connection);
		}

		public bool GetIsLoggedIn (IUserInfo user)
		{
			if (user == null)
				throw new ArgumentNullException ("user");

			var c = GetConnection (user);
			if (c != null)
				return loggedIn.Contains (c);

			return false;
		}

		public bool GetIsNicknameInUse (string nickname)
		{
			if (nickname == null)
				throw new ArgumentNullException ("nickname");

			nickname = nickname.ToLower().Trim();

			return connectedUsers.Keys.Where (u => u.Nickname != null)
				.Any (u => u.Nickname.ToLower().Trim() == nickname);
		}

		public void Disconnect (IConnection connection)
		{
			if (connection == null)
				throw new ArgumentNullException ("connection");

			if (!connections.Remove (connection))
				return;
			
			loggedIn.Remove (connection);
			joined.Remove (connection);
			connectedUsers.Inverse.Remove (connection);
		}

		public void Disconnect (Func<IConnection, bool> predicate)
		{
			if (predicate == null)
				throw new ArgumentNullException ("predicate");

			foreach (IConnection c in this.connections.Where (predicate).ToList())
			{
				connections.Remove (c);
				loggedIn.Remove (c);
				joined.Remove (c);
				connectedUsers.Inverse.Remove (c);
			}
		}

		public IUserInfo GetUser (IConnection connection)
		{
			if (connection == null)
				throw new ArgumentNullException ("connection");

			IUserInfo user;
			connectedUsers.TryGetKey (connection, out user);

			return user;
		}

		public IConnection GetConnection (IUserInfo user)
		{
			if (user == null)
				throw new ArgumentNullException ("user");

			IConnection connection;
			connectedUsers.TryGetValue (user, out connection);

			return connection;
		}

		#endregion

		private readonly HashSet<IConnection> joined = new HashSet<IConnection>();
		private readonly HashSet<IConnection> loggedIn = new HashSet<IConnection>();
		private readonly HashSet<IConnection> connections = new HashSet<IConnection>();
		private readonly BidirectionalDictionary<IUserInfo, IConnection> connectedUsers = new BidirectionalDictionary<IUserInfo, IConnection> (10, new UserEqualityComparer(), EqualityComparer<IConnection>.Default);

		#region IEnumerable Members

		public IEnumerator<IUserInfo> GetEnumerator()
		{
			return connectedUsers.Keys.ToList().GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		#endregion
	}
}