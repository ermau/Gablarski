// Copyright (c) 2010, Eric Maupin
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
using System.Collections.Generic;
using System.Linq;
using Cadenza.Collections;

namespace Gablarski.Client
{
	internal class ClientUserManager
		: IClientUserManager
	{
		public IUserInfo this[int userId]
		{
			get
			{
				UserInfo user;
				lock (syncRoot)
				{
					this.users.TryGetValue (userId, out user);
				}

				return user;
			}
		}

		public void Join (IUserInfo user)
		{
			if (user == null)
				throw new ArgumentNullException ("user");

			var u = new UserInfo (user);
			lock (syncRoot)
			{
				users.Add (user.UserId, u);
				channels.Add (user.CurrentChannelId, u);
			}
		}

		public bool GetIsJoined (IUserInfo user)
		{
			if (user == null)
				throw new ArgumentNullException ("user");

			lock (syncRoot)
				return users.ContainsKey (user.UserId);
		}

		public bool GetIsIgnored (IUserInfo user)
		{
			if (user == null)
				throw new ArgumentNullException ("user");

			lock (this.syncRoot)
				return this.ignores.Contains (user.UserId);
		}

		public bool Depart (IUserInfo user)
		{
			if (user == null)
				throw new ArgumentNullException ("user");

			lock (this.syncRoot)
			{
				this.ignores.Remove (user.UserId);

				UserInfo realUser;
				if (this.users.TryGetValue (user.UserId, out realUser))
				{
					this.users.Remove (user.UserId);
					return this.channels.Remove (user.CurrentChannelId, realUser);
				}
				else
					return false;
			}
		}
		
		public void Update (IEnumerable<IUserInfo> userUpdate)
		{
			if (userUpdate == null)
				throw new ArgumentNullException ("userUpdate");
				
			lock (this.syncRoot)
			{
				this.ignores = new HashSet<int> (this.ignores.Intersect (userUpdate.Select (u => u.UserId)));
				this.users = userUpdate.ToDictionary (u => u.UserId, u => new UserInfo (u));
				this.channels =
					new MutableLookup<int, UserInfo> (userUpdate.ToLookup (u => u.CurrentChannelId, u => new UserInfo (u)));
			}
		}
		
		public void Update (IUserInfo user)
		{
			if (user == null)
				throw new ArgumentNullException ("user");
				
			lock (this.syncRoot)
			{
				UserInfo oldUser;
				if (users.TryGetValue (user.UserId, out oldUser))
					channels.Remove (oldUser.CurrentChannelId, oldUser);
					
				channels.Add (user.CurrentChannelId, users[user.UserId] = new UserInfo (user));
			}
		}

		public void Clear ()
		{
			lock (this.syncRoot)
			{
				ignores.Clear();
				users.Clear();
				channels.Clear();
			}
		}

		public IEnumerable<IUserInfo> GetUsersInChannel (int channelId)
		{
			lock (this.syncRoot)
			{
				return this.channels[channelId].Cast<IUserInfo>().ToList();
			}
		}

		public bool ToggleIgnore (IUserInfo user)
		{
			if (user == null)
				throw new ArgumentNullException ("user");

			bool ignored;
			lock (this.syncRoot)
			{
				if (!(ignored = ignores.Remove (user.UserId)))
					ignores.Add (user.UserId);
			}

			return !ignored;
		}

		public void ToggleMute (IUserInfo user)
		{
			if (user == null)
				throw new ArgumentNullException ("user");

			lock (this.syncRoot)
			{
				UserInfo oldUser;
				if (!users.TryGetValue (user.UserId, out oldUser))
					oldUser = new UserInfo (user);
				
				users[user.UserId] = new UserInfo (oldUser.Nickname, user.Phonetic, user.Username, user.UserId, user.CurrentChannelId, !user.IsMuted);
			}
		}

		public IEnumerator<IUserInfo> GetEnumerator ()
		{
			IEnumerable<IUserInfo> returnUsers;
			lock (this.syncRoot)
			{
				 returnUsers = this.users.Values.Cast<IUserInfo>().ToList();
			}

			return returnUsers.GetEnumerator ();
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator ()
		{
			return GetEnumerator ();
		}

		object IClientUserManager.SyncRoot
		{
			get { return this.syncRoot; }
		}

		private readonly object syncRoot = new object ();

		private HashSet<int> ignores = new HashSet<int>();
		private Dictionary<int, UserInfo> users = new Dictionary<int, UserInfo> ();
		private MutableLookup<int, UserInfo> channels = new MutableLookup<int, UserInfo> ();
	}
}