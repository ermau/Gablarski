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

namespace Gablarski.Client
{
	public interface IClientUserHandler
		: IIndexedEnumerable<int, UserInfo>
	{
		/// <summary>
		/// An new or updated user list has been received.
		/// </summary>
		event EventHandler<ReceivedListEventArgs<UserInfo>> ReceivedUserList;
		
		/// <summary>
		/// A new user has joined.
		/// </summary>
		event EventHandler<UserEventArgs> UserJoined;
		
		/// <summary>
		/// A user has disconnected.
		/// </summary>
		event EventHandler<UserEventArgs> UserDisconnected;
		
		/// <summary>
		/// A user was muted or ignored.
		/// </summary>
		event EventHandler<UserMutedEventArgs> UserMuted;

		/// <summary>
		/// An existing user's information was updated.
		/// </summary>
		event EventHandler<UserEventArgs> UserUpdated;

		/// <summary>
		/// A user has changed channels.
		/// </summary>
		event EventHandler<ChannelChangedEventArgs> UserChangedChannel;

		/// <summary>
		/// Gets the current user.
		/// </summary>
		CurrentUser Current { get; }

		/// <summary>
		/// Gets whether the user has been ignored by the current user.
		/// </summary>
		/// <param name="user">The user to check.</param>
		/// <returns><c>true</c> if the user is ignored, <c>false</c> if not.</returns>
		bool GetIsIgnored (UserInfo user);

		/// <summary>
		/// Toggles ignore on <paramref name="user"/>.
		/// </summary>
		/// <param name="user">The user to ignore or unignore.</param>
		/// <returns><c>true</c> if the user is now ignored, <c>false</c> if the user is now unignored.</returns>
		bool ToggleIgnore (UserInfo user);
		
		/// <summary>
		/// Tries to get <parmref name="user"/> from <paramref name="userId"/>.
		/// </summary>
		/// <param name="userId">The id of the user to try to get.</param>
		/// <param name="user">The user, if found.</param>
		/// <returns><c>true</c> if the user was found, <c>false</c> otherwise.</returns>
		bool TryGetUser (int userId, out UserInfo user);

		/// <summary>
		/// Gets the users in the given channel.
		/// </summary>
		/// <param name="channelId">The id of the channel.</param>
		/// <returns>
		/// A <see cref="IEnumerable{UserInfo}"/> of the users in the channel. <c>null</c> if the channel was not found.
		/// </returns>
		IEnumerable<UserInfo> GetUsersInChannel (int channelId);

		/// <summary>
		/// Requests to move <paramref name="user"/> to <paramref name="targetChannel"/>.
		/// </summary>
		/// <param name="user">The user to move.</param>
		/// <param name="targetChannel">The target channel to move the user to.</param>
		void Move (UserInfo user, ChannelInfo targetChannel);

		/// <summary>
		/// Attempts to toggle mute on <paramref name="user"/>.
		/// </summary>
		/// <param name="user">The user to attempt to mute.</param>
		/// <exception cref="ArgumentNullException"><paramref name="user"/> is <c>null</c>.</exception>
		/// <remarks>
		/// Since muting is server-side and permissions restricted, this will return immediately without
		/// new state information, that'll come later.
		/// </remarks>
		void ToggleMute (UserInfo user);

		/// <summary>
		/// Resets the handler to it's initial state.
		/// </summary>
		/// <remarks>
		/// Integraters shouldn't invoke this directly, it's for the <see cref="GablarskiClient"/> to do
		/// when disconnecting.
		/// </remarks>
		void Reset();
	}
}