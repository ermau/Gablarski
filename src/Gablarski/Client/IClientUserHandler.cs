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
using System.Threading.Tasks;

namespace Gablarski.Client
{
	public interface IClientUserHandler
		: IIndexedEnumerable<int, IUserInfo>
	{
		/// <summary>
		/// An new or updated user list has been received.
		/// </summary>
		event EventHandler<ReceivedListEventArgs<IUserInfo>> ReceivedUserList;
		
		/// <summary>
		/// A new user has joined.
		/// </summary>
		event EventHandler<UserEventArgs> UserJoined;
		
		/// <summary>
		/// A user has disconnected.
		/// </summary>
		event EventHandler<UserEventArgs> UserDisconnected;
		
		/// <summary>
		/// A user was muted.
		/// </summary>
		event EventHandler<UserMutedEventArgs> UserMuted;

		/// <summary>
		/// A user was ignored.
		/// </summary>
		event EventHandler<UserMutedEventArgs> UserIgnored;

		/// <summary>
		/// An existing user's information was updated.
		/// </summary>
		event EventHandler<UserEventArgs> UserUpdated;

		/// <summary>
		/// A user has changed channels.
		/// </summary>
		event EventHandler<ChannelChangedEventArgs> UserChangedChannel;

		/// <summary>
		/// A user was kicked from their current channel to the default channel.
		/// </summary>
		event EventHandler<UserKickedEventArgs> UserKicked;

		/// <summary>
		/// Gets the current user.
		/// </summary>
		IUserInfo Current { get; }

		/// <summary>
		/// Gets a lookup for users by channel id.
		/// </summary>
		ILookup<int, IUserInfo> ByChannel { get; }

		object SyncRoot { get; }

		/// <summary>
		/// Gets whether or not <paramref name="user"/> is currently ignored.
		/// </summary>
		/// <param name="user">The user to check.</param>
		/// <returns><c>true</c> if ignored, <c>false</c> if not.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="user"/> is <c>null</c>.</exception>
		bool GetIsIgnored (IUserInfo user);

		/// <summary>
		/// Toggles ignore on <paramref name="user"/>.
		/// </summary>
		/// <param name="user">The user to ignore or unignore.</param>
		/// <returns><c>true</c> if the user is now ignored, <c>false</c> if the user is now unignored.</returns>
		bool ToggleIgnore (IUserInfo user);

		/// <summary>
		/// Approves the registration of <paramref name="username"/>.
		/// </summary>
		/// <param name="username">The username to approve registration for.</param>
		/// <exception cref="ArgumentNullException"><paramref name="username"/> is <c>null</c>.</exception>
		/// <exception cref="NotSupportedException">The server does not support either registration or approvals.</exception>
		Task ApproveRegistrationAsync (string username);

		/// <summary>
		/// Pre-approves the registration of <paramref name="user"/>.
		/// </summary>
		/// <param name="user">The user to pre-approve for registration.</param>
		/// <exception cref="ArgumentNullException"><paramref name="user"/> is <c>null</c>.</exception>
		/// <exception cref="NotSupportedException">The server does not support either pre-registration or approvals.</exception>
		Task ApproveRegistrationAsync (IUserInfo user);

		/// <summary>
		/// Rejects the registration of <paramref name="username"/>.
		/// </summary>
		/// <param name="username">The username to reject registration for.</param>
		/// <exception cref="ArgumentNullException"><paramref name="username"/> is <c>null</c>.</exception>
		/// <exception cref="NotSupportedException">The server does not support either registration or approvals.</exception>
		Task RejectRegistrationAsync (string username);

		/// <summary>
		/// Requests to move <paramref name="user"/> to <paramref name="targetChannel"/>.
		/// </summary>
		/// <param name="user">The user to move.</param>
		/// <param name="targetChannel">The target channel to move the user to.</param>
		Task MoveAsync (IUserInfo user, IChannelInfo targetChannel);

		/// <summary>
		/// Kicks a user to the default channel or from the server.
		/// </summary>
		/// <param name="user">The user to kick.</param>
		/// <param name="fromServer">Whether to kick the user from the server or just from the default channel.</param>
		/// <exception cref="ArgumentNullException"><paramref name="user"/> is <c>null</c>.</exception>
		Task KickAsync (IUser user, bool fromServer);

		Task BanAsync (IUserInfo user, TimeSpan banLength);

		/// <summary>
		/// Attempts to toggle mute on <paramref name="user"/>.
		/// </summary>
		/// <param name="user">The user to attempt to mute/unmute.</param>
		/// <param name="mute">Whether to mute or unmute.</param>
		/// <exception cref="ArgumentNullException"><paramref name="user"/> is <c>null</c>.</exception>
		Task SetMuteAsync (IUserInfo user, bool mute);

		/// <summary>
		/// Resets the handler to it's initial state.
		/// </summary>
		/// <remarks>
		/// Integrators shouldn't invoke this directly, it's for the <see cref="GablarskiClient"/> to do
		/// when disconnecting.
		/// </remarks>
		void Reset();
	}
}