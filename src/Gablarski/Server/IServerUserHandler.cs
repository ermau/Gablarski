//
// IServerUserHandler.cs
//
// Author:
//   Eric Maupin <me@ermau.com>
//
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
using System.Linq;
using System.Threading.Tasks;
using Gablarski.Messages;
using Tempest;

namespace Gablarski.Server
{
	public interface IServerUserHandler
		: IIndexedEnumerable<int, IUserInfo>
	{
		/// <summary>
		/// Gets the user associated with this connection.
		/// </summary>
		/// <param name="connection">The connection to look up a user for.</param>
		/// <returns>The user associated with <paramref name="connection"/> or <c>null</c> if none found.</returns>
		IUserInfo this [IConnection connection] { get; }

		/// <summary>
		/// Gets the connection associated with this user.
		/// </summary>
		/// <param name="user">The user to look up a connection for.</param>
		/// <returns>The connection associated with <paramref name="user"/> or <c>null</c> if none found.</returns>
		IServerConnection this [IUserInfo user] { get; }

		/// <summary>
		/// Gets all connections.
		/// </summary>
		IEnumerable<IConnection> Connections { get; }

		/// <summary>
		/// Gets the connections for joined users.
		/// </summary>
		IEnumerable<IConnection> UserConnections { get; }

		/// <summary>
		/// Moves <paramref name="user"/> to the <paramref name="targetChannel"/>.
		/// </summary>
		/// <param name="mover">The connection actually moving the user.</param>
		/// <param name="user">The user to move.</param>
		/// <param name="targetChannel">The channel to move the user to.</param>
		/// <exception cref="ArgumentNullException"><paramref name="mover"/> or <paramref name="user" /> or <paramref name="targetChannel"/> is <c>null</c>.</exception>
		void Move (IConnection mover, IUserInfo user, IChannelInfo targetChannel);

		/// <summary>
		/// Moves <paramref name="user"/> to the <paramref name="targetChannel"/>.
		/// </summary>
		/// <param name="user">The user to move.</param>
		/// <param name="targetChannel">The channel to move the user to.</param>
		/// <exception cref="ArgumentNullException"><paramref name="user" /> or <paramref name="targetChannel"/> is <c>null</c>.</exception>
		/// <remarks>
		/// This should only be used by systems code that needs to forcibly move a user.
		/// Ex. <see cref="IChannelProvider.ChannelsUpdated"/> results in a populated channel being removed.
		/// </remarks>
		void Move (IUserInfo user, IChannelInfo targetChannel);

		/// <summary>
		/// Pre-approves registration for <paramref name="user"/>.
		/// </summary>
		/// <param name="user">The user to pre-approve for registration.</param>
		/// <exception cref="ArgumentNullException"><paramref name="user"/> is <c>null</c>.</exception>
		void ApproveRegistration (IUserInfo user);

		/// <summary>
		/// Approves the awaiting registration with the <paramref name="username"/>.
		/// </summary>
		/// <param name="username">The username for the awaiting registration.</param>
		/// <exception cref="ArgumentNullException"><paramref name="username"/> is <c>null</c>.</exception>
		void ApproveRegistration (string username);

		/// <summary>
		/// Disconnects a user for a specific reason.
		/// </summary>
		/// <param name="user">The user to disconnect.</param>
		/// <param name="reason">The reason to disconnect the user.</param>
		/// <exception cref="ArgumentNullException"><paramref name="user"/> is <c>null</c>.</exception>
		Task DisconnectAsync (IUserInfo user, DisconnectionReason reason);
	}
}