// Copyright (c) 2009-2014, Eric Maupin
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
		: IIndexedEnumerable<int, IUserInfo>
	{
		public int Count
		{
			get
			{
				lock (this.syncRoot)
					return this.users.Count;
			}
		}

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

		public object SyncRoot
		{
			get { return this.syncRoot; }
		}

		/// <summary>
		/// Joins <paramref name="user"/>.
		/// </summary>
		/// <param name="user">
		/// A <see cref="UserInfo"/>
		/// </param>
		/// <exception cref="ArgumentNullException"><paramref name="user"/> is <c>null</c>.</exception>
		public void Join (IUserInfo user)
		{
			if (user == null)
				throw new ArgumentNullException ("user");

			var u = new UserInfo (user);
			Update (u);
		}

		/// <summary>
		/// Gets whether or not <paramref name="user"/> is currently in the manager.
		/// </summary>
		/// <param name="user">The user to check for.</param>
		/// <returns><c>true</c> if <paramref name="user"/> is in the manager, <c>false</c> otherwise.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="user"/> is <c>null</c>.</exception>
		public bool GetIsJoined (IUserInfo user)
		{
			if (user == null)
				throw new ArgumentNullException ("user");

			lock (syncRoot)
				return users.ContainsKey (user.UserId);
		}

		/// <summary>
		/// Gets whether or not <paramref name="user"/> is currently ignored.
		/// </summary>
		/// <param name="user">The user to check.</param>
		/// <returns><c>true</c> if ignored, <c>false</c> if not.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="user"/> is <c>null</c>.</exception>
		public bool GetIsIgnored (IUserInfo user)
		{
			if (user == null)
				throw new ArgumentNullException ("user");

			lock (this.syncRoot)
				return this.ignores.Contains (user.UserId);
		}

		/// <summary>
		/// Departs a user.
		/// </summary>
		/// <exception cref="ArgumentNullException"><paramref name="user"/> is <c>null</c></exception>
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
		
		/// <summary>
		/// Updates the manager using <paramref name="userUpdate"/> as the new list of users.
		/// </summary>
		/// <param name="userUpdate">The new list of users.</param>
		/// <exception cref="ArgumentNullException"><paramref name="userUpdate"/> is <c>null</c></exception>
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
		
		/// <summary>
		/// Updates the user internally to match the properties of <paramref name="user"/>.
		/// </summary>
		/// <param name="user">The new set of properties for the user.</param>
		/// <exception cref="ArgumentNullException"><paramref name="user"/> is <c>null</c>.</exception>
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

		/// <summary>
		/// Gets the users in the given channel.
		/// </summary>
		/// <param name="channelId">The id of the channel.</param>
		/// <returns>
		/// A <see cref="IEnumerable{UserInfo}"/> of the users in the channel. <c>null</c> if the channel was not found.
		/// </returns>
		public IEnumerable<IUserInfo> GetUsersInChannel (int channelId)
		{
			lock (this.syncRoot)
			{
				return this.channels[channelId].Cast<IUserInfo>().ToList();
			}
		}

		/// <summary>
		/// Toggles ignore on <paramref name="user"/>.
		/// </summary>
		/// <param name="user">The user to toggle ignore for.</param>
		/// <returns>The new ignore state.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="user"/> is <c>null</c>.</exception>
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

		/// <summary>
		/// Toggles mute on <paramref name="user"/>.
		/// </summary>
		/// <param name="user">The user to toggle mute for.</param>
		/// <exception cref="ArgumentNullException"><paramref name="user"/> is <c>null</c>.</exception>
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

		private readonly object syncRoot = new object ();

		private HashSet<int> ignores = new HashSet<int>();
		private Dictionary<int, UserInfo> users = new Dictionary<int, UserInfo> ();
		private MutableLookup<int, UserInfo> channels = new MutableLookup<int, UserInfo> ();
	}
}