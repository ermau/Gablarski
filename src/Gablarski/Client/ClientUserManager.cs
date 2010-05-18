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
using System.Collections.Generic;
using System.Linq;
using Cadenza.Collections;

namespace Gablarski.Client
{
	internal class ClientUserManager
		: IClientUserManager
	{
		public UserInfo this[int userId]
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

		public void Join (UserInfo user)
		{
			if (user == null)
				throw new ArgumentNullException ("user");

			lock (syncRoot)
			{
				users.Add (user.UserId, user);
				channels.Add (user.CurrentChannelId, user);
			}
		}

		public bool GetIsJoined (UserInfo user)
		{
			if (user == null)
				throw new ArgumentNullException ("user");

			lock (syncRoot)
				return users.ContainsValue (user);
		}

		public bool GetIsIgnored (UserInfo user)
		{
			if (user == null)
				throw new ArgumentNullException ("user");

			lock (this.syncRoot)
				return this.ignores.Contains (user);
		}

		public bool Depart (UserInfo user)
		{
			if (user == null)
				throw new ArgumentNullException ("user");

			lock (this.syncRoot)
			{
				this.ignores.Remove (user);
				this.users.Remove (user.UserId);
				return this.channels.Remove (user.CurrentChannelId, user);
			}
		}
		
		public void Update (IEnumerable<UserInfo> userUpdate)
		{
			if (userUpdate == null)
				throw new ArgumentNullException ("userUpdate");
				
			lock (this.syncRoot)
			{
				this.ignores = new HashSet<UserInfo> (this.ignores.Intersect (userUpdate));
				this.users = userUpdate.ToDictionary (u => u.UserId);
				this.channels = new MutableLookup<int, UserInfo> (userUpdate.ToLookup (u => u.CurrentChannelId));
			}
		}
		
		public void Update (UserInfo user)
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

		public IEnumerable<UserInfo> GetUsersInChannel (int channelId)
		{
			lock (this.syncRoot)
			{
				return this.channels[channelId].ToList();
			}
		}

		public bool ToggleIgnore (UserInfo user)
		{
			if (user == null)
				throw new ArgumentNullException ("user");

			bool ignored;
			lock (this.syncRoot)
			{
				ignored = ignores.Contains (user);

				if (ignored)
					ignores.Remove (user);
				else
					ignores.Add (user);
			}

			return !ignored;
		}

		public void ToggleMute (UserInfo user)
		{
			if (user == null)
				throw new ArgumentNullException ("user");

			lock (this.syncRoot)
			{
				UserInfo oldUser;
				if (!users.TryGetValue (user.UserId, out oldUser))
					oldUser = user;
				
				users[user.UserId] = new UserInfo (oldUser.Nickname, user.Phonetic, user.Username, user.UserId, user.CurrentChannelId, !user.IsMuted);
			}
		}

		public IEnumerator<UserInfo> GetEnumerator ()
		{
			IEnumerable<UserInfo> returnUsers;
			lock (this.syncRoot)
			{
				 returnUsers = this.users.Values.ToList();
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

		private HashSet<UserInfo> ignores = new HashSet<UserInfo>();
		private Dictionary<int, UserInfo> users = new Dictionary<int, UserInfo> ();
		private MutableLookup<int, UserInfo> channels = new MutableLookup<int, UserInfo> ();
	}
}