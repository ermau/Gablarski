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
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gablarski.Client
{
	public interface IClientUserManager
		: IIndexedEnumerable<int, IUserInfo>
	{
		/// <summary>
		/// Gets the synchronization root for the manager.
		/// </summary>
		object SyncRoot { get; }

		/// <summary>
		/// Joins <paramref name="user"/>.
		/// </summary>
		/// <param name="user">
		/// A <see cref="UserInfo"/>
		/// </param>
		/// <exception cref="ArgumentNullException"><paramref name="user"/> is <c>null</c>.</exception>
		void Join (IUserInfo user);

		/// <summary>
		/// Gets whether or not <paramref name="user"/> is currently in the manager.
		/// </summary>
		/// <param name="user">The user to check for.</param>
		/// <returns><c>true</c> if <paramref name="user"/> is in the manager, <c>false</c> otherwise.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="user"/> is <c>null</c>.</exception>
		bool GetIsJoined (IUserInfo user);

		/// <summary>
		/// Gets whether or not <paramref name="user"/> is currently ignored.
		/// </summary>
		/// <param name="user">The user to check.</param>
		/// <returns><c>true</c> if ignored, <c>false</c> if not.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="user"/> is <c>null</c>.</exception>
		bool GetIsIgnored (IUserInfo user);
		
		/// <summary>
		/// Departs a user.
		/// </summary>
		/// <exception cref="ArgumentNullException"><paramref name="user"/> is <c>null</c></exception>
		bool Depart (IUserInfo user);
		
		/// <summary>
		/// Updates the manager using <paramref name="users"/> as the new list of users.
		/// </summary>
		/// <param name="users">The new list of users.</param>
		/// <exception cref="ArgumentNullException"><paramref name="users"/> is <c>null</c></exception>
		void Update (IEnumerable<IUserInfo> users);
		
		/// <summary>
		/// Updates the user internally to match the properties of <paramref name="user"/>.
		/// </summary>
		/// <param name="user">The new set of properties for the user.</param>
		/// <exception cref="ArgumentNullException"><paramref name="user"/> is <c>null</c>.</exception>
		void Update (IUserInfo user);

		/// <summary>
		/// Gets the users in the given channel.
		/// </summary>
		/// <param name="channelId">The id of the channel.</param>
		/// <returns>
		/// A <see cref="IEnumerable{UserInfo}"/> of the users in the channel. <c>null</c> if the channel was not found.
		/// </returns>
		IEnumerable<IUserInfo> GetUsersInChannel (int channelId);

		/// <summary>
		/// Toggles mute on <paramref name="user"/>.
		/// </summary>
		/// <param name="user">The user to toggle mute for.</param>
		/// <exception cref="ArgumentNullException"><paramref name="user"/> is <c>null</c>.</exception>
		void ToggleMute (IUserInfo user);

		/// <summary>
		/// Toggles ignore on <paramref name="user"/>.
		/// </summary>
		/// <param name="user">The user to toggle ignore for.</param>
		/// <returns>The new ignore state.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="user"/> is <c>null</c>.</exception>
		bool ToggleIgnore (IUserInfo user);
	}

	public static class UserManagerExtensions
	{
		public static void Clear (this IClientUserManager self)
		{
			if (self == null)
				throw new ArgumentNullException ("self");

			self.Update (Enumerable.Empty<IUserInfo>());
		}
	}
}