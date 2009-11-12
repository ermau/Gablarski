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
using System.Text;
using Mono.Rocks;

namespace Gablarski
{
	public class UserManager
		: IUserManager
	{
		public void Add (UserInfo user)
		{
			if (user == null)
				throw new ArgumentNullException ("user");

			lock (userLock)
			{
				users.Add (user.UserId, user);
				channels.Add (user.CurrentChannelId, user);
			}
		}

		public bool Contains (UserInfo user)
		{
			if (user == null)
				throw new ArgumentNullException ("user");

			lock (userLock)
			{
				return users.ContainsValue (user);
			}
		}

		public bool Remove (UserInfo user)
		{
			if (user == null)
				throw new ArgumentNullException ("user");

			lock (userLock)
			{
				users.Remove (user.UserId);
				return channels.Remove (user.CurrentChannelId, user);
			}
		}

		public void Clear ()
		{
			lock (userLock)
			{
				users.Clear();
				channels.Clear();
			}
		}

		public void Update (IEnumerable<UserInfo> users, out IEnumerable<UserInfo> added, out IEnumerable<UserInfo> removed, out IEnumerable<UserInfo> changed)
		{
			throw new NotImplementedException();
		}

		public IEnumerable<UserInfo> GetUsersInChannel (int channelId)
		{
			lock (users)
			{
				return this.channels[channelId].ToList();
			}
		}

		public void ToggleMute (UserInfo user)
		{
			if (user == null)
				throw new ArgumentNullException ("user");

			throw new NotImplementedException ();
		}

		public UserInfo this[int key]
		{
			get
			{
				UserInfo user;
				lock (userLock)
				{
					this.users.TryGetValue (key, out user);
				}

				return user;
			}
		}

		public IEnumerator<UserInfo> GetEnumerator ()
		{
			IEnumerable<UserInfo> returnUsers;
			lock (users)
			{
				 returnUsers = this.users.Values.ToList();
			}

			return returnUsers.GetEnumerator ();
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator ()
		{
			return GetEnumerator ();
		}

		private readonly object userLock = new object ();

		private readonly Dictionary<int, UserInfo> users = new Dictionary<int, UserInfo> ();
		private readonly MutableLookup<int, UserInfo> channels = new MutableLookup<int, UserInfo> ();
	}
}