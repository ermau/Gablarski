// Copyright (c) 2009-2011, Eric Maupin
// Copyright (c) 2011-2014, Xamarin Inc.
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
using System.Collections.Specialized;
using System.Linq;
using Cadenza.Collections;

namespace Gablarski.Client
{
	internal class ClientUserManager
		: IReadOnlyDictionary<int, IUserInfo>, INotifyCollectionChanged
	{
		public event NotifyCollectionChangedEventHandler CollectionChanged;

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
				IUserInfo user;
				lock (syncRoot) {
					this.users.TryGetValue (userId, out user);
				}

				return user;
			}
		}

		public IEnumerable<int> Keys
		{
			get { return this.users.Keys; }
		}

		public IEnumerable<IUserInfo> Values
		{
			get { return this.users.Values; }
		}

		public ILookup<int, IUserInfo> ByChannel
		{
			get { return this.channels; }
		}

		public object SyncRoot
		{
			get { return this.syncRoot; }
		}

		public bool TryGetValue (int key, out IUserInfo value)
		{
			lock (this.syncRoot)
				return this.users.TryGetValue (key, out value);
		}

		public bool ContainsKey (int key)
		{
			lock (this.syncRoot)
				return this.users.ContainsKey (key);
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

			int index = -1;
			lock (this.syncRoot) {
				this.ignores.Remove (user.UserId);

				IUserInfo realUser;
				if (!this.users.TryGetValue (user.UserId, out realUser))
					return false;

				index = this.users.IndexOf (user.UserId);
				this.users.Remove (user.UserId);
				this.channels.Remove (user.CurrentChannelId, realUser);
			}

			OnCollectionChanged (new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Remove, user, index));
			return true;
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

			var update = userUpdate.ToDictionary (u => u.UserId, u => (IUserInfo) new UserInfo (u));

			lock (this.syncRoot) {
				var intersectIgnores = this.ignores.Intersect (update.Keys).ToArray();
				
				ClearCore();

				foreach (int ignoreId in intersectIgnores)
					this.ignores.Add (ignoreId);

				foreach (var kvp in update) {
					this.users.Add (kvp.Key, kvp.Value);
					this.channels.Add (kvp.Value.CurrentChannelId, kvp.Value);
				}
			}

			OnCollectionChanged (new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Reset));
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

			user = new UserInfo (user);

			int index = 0;
			bool removed = false;
			IUserInfo oldUser;
			lock (this.syncRoot) {
				if (users.TryGetValue (user.UserId, out oldUser)) {
					index = this.users.IndexOf (user.UserId);
					this.users.Remove (user.UserId);
					this.channels.Remove (oldUser.CurrentChannelId, oldUser);
					removed = true;
				} else
					index = this.users.Count;

				this.users.Insert (index, user.UserId, user);
				channels.Add (user.CurrentChannelId, user);
			}

			if (removed)
				OnCollectionChanged (new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Replace, oldUser, user, index));
			else
				OnCollectionChanged (new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Add, user, index));
		}

		public void Clear ()
		{
			ClearCore();
			OnCollectionChanged (new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Reset));
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

			lock (this.syncRoot) {
				IUserInfo oldUser;
				if (!users.TryGetValue (user.UserId, out oldUser))
					oldUser = new UserInfo (user);
				
				Update (new UserInfo (oldUser.Nickname, user.Phonetic, user.Username, user.UserId, user.CurrentChannelId, !user.IsMuted));
			}
		}

		public IEnumerator<KeyValuePair<int, IUserInfo>> GetEnumerator()
		{
			return this.users.GetEnumerator();
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator ()
		{
			return GetEnumerator ();
		}

		private readonly object syncRoot = new object ();

		private readonly HashSet<int> ignores = new HashSet<int>();
		private readonly OrderedDictionary<int, IUserInfo> users = new OrderedDictionary<int, IUserInfo> ();
		private readonly ObservableLookup<int, IUserInfo> channels = new ObservableLookup<int, IUserInfo> (persistCollections: true);

		private void ClearCore()
		{
			lock (this.syncRoot) {
				ignores.Clear();
				users.Clear();
				channels.Clear();
			}
		}

		private void OnCollectionChanged (NotifyCollectionChangedEventArgs e)
		{
			var handler = CollectionChanged;
			if (handler != null)
				handler (this, e);
		}
	}
}