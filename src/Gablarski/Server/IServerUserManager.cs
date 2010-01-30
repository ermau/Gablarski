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
using Gablarski.Messages;

namespace Gablarski.Server
{
	public interface IServerUserManager
		: IConnectionManager, IIndexedEnumerable<int, UserInfo>
	{
		/// <exception cref="ArgumentNullException"><paramref name="connection"/> or <paramref name="user"/> is <c>null</c>.</exception>
		void Login (IConnection connection, UserInfo user);

		/// <exception cref="ArgumentNullException"><paramref name="user"/> is <c>null</c>.</exception>
		bool GetIsLoggedIn (UserInfo user);

		/// <exception cref="ArgumentNullException"><paramref name="connection"/> is <c>null</c>.</exception>
		bool GetIsLoggedIn (IConnection connection);

		/// <exception cref="ArgumentNullException"><paramref name="connection"/> or <paramref name="user"/> is <c>null</c>.</exception>
		void Join (IConnection connection, UserInfo user);
		
		/// <exception cref="ArgumentNullException"><paramref name="user"/> is <c>null</c>.</exception>
		bool GetIsJoined (UserInfo user);

		/// <exception cref="ArgumentNullException"><paramref name="connection"/> is <c>null</c>.</exception>
		bool GetIsJoined (IConnection connection);

		/// <summary>
		/// Moves <paramref name="user"/> to <paramref name="channel"/>.
		/// </summary>
		/// <param name="user">The user to move.</param>
		/// <param name="channel">The channel to move the user to.</param>
		/// <exception cref="ArgumentNullException"><paramref name="user"/> or <paramref name="channel"/> is <c>null</c>.</exception>
		void Move (UserInfo user, ChannelInfo channel);

		/// <summary>
		/// Toggles mute on the <paramref name="user"/>.
		/// </summary>
		/// <param name="user">The user to mute or unmute.</param>
		/// <returns><c>true</c> if <paramref name="user"/> is now muted, <c>false</c> otherwise.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="user"/> is <c>null</c>.</exception>
		bool ToggleMute (UserInfo user);

		/// <summary>
		/// Sets the state for the <paramref name="user"/>.
		/// </summary>
		/// <param name="user">The user to set the state of.</param>
		/// <param name="newState">The new state of the user.</param>
		/// <exception cref="ArgumentNullException"><paramref name="user"/> is <c>null</c>.</exception>
		void SetState (UserInfo user, UserState newState);

		/// <summary>
		/// Gets whether <paramref name="nickname"/> is currently in use or not.
		/// </summary>
		/// <param name="nickname">The name to check.</param>
		/// <returns><c>true</c> if the nickname is in use, <c>false</c> otherwise.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="nickname"/> is <c>null</c>.</exception>
		/// <remarks>
		/// Nicknames are trimmed and lower cased before checking, changing case or adding
		/// spaces will not result in an unused nickname.
		/// </remarks>
		bool GetIsNicknameInUse (string nickname);

		/// <summary>
		/// Gets the connection associated with the <paramref name="user"/>. <c>null</c> if not found.
		/// </summary>
		/// <param name="user">The user to find the connection for.</param>
		/// <returns>The associated connection, <c>null</c> if not found.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="user"/> is <c>null</c>.</exception>
		IConnection GetConnection (UserInfo user);

		/// <summary>
		/// Gets the user associated with the <paramref name="connection"/>. <c>null</c> if not found.
		/// </summary>
		/// <param name="connection"></param>
		/// <returns>The associated user, <c>null</c> if not found.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="connection"/> is <c>null</c>.</exception>
		UserInfo GetUser (IConnection connection);
	}

	public static class ServerUserManagerExtensions
	{
		public static void Disconnect (this IServerUserManager self, UserInfo user)
		{
			if (self == null)
				throw new ArgumentNullException ("self");

			var c = self.GetConnection (user);
			if (c != null)
				self.Disconnect (c);
		}
	}
}