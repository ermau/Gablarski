﻿// Copyright (c) 2010, Eric Maupin
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
using Gablarski.Messages;

namespace Gablarski.Server
{
	public interface IServerUserHandler
		: IConnectionHandler, IIndexedEnumerable<int, IUserInfo>
	{
		/// <summary>
		/// Moves <paramref name="user"/> to the <paramref name="targetChannel"/>.
		/// </summary>
		/// <param name="user">The user to move.</param>
		/// <param name="targetChannel">The channel to move the user to.</param>
		/// <exception cref="ArgumentNullException"><paramref name="user" /> or <paramref name="targetChannel"/> is <c>null</c>.</exception>
		/// <remarks>
		/// The main reasons for a failed move are that the user or channel no longer exists.
		/// </remarks>
		void Move (IUserInfo user, ChannelInfo targetChannel);

		void Send (MessageBase message, Func<IConnection, IUserInfo, bool> predicate);

		/// <summary>
		/// Disconnects a user for a specific reason.
		/// </summary>
		/// <param name="user">The user to disconnect.</param>
		/// <param name="reason">The reason to disconnect the user.</param>
		/// <exception cref="ArgumentNullException"><paramref name="user"/> is <c>null</c>.</exception>
		void Disconnect (IUserInfo user, DisconnectionReason reason);
	}

	public static class ServerUserHandlerExtensions
	{
		public static void Send (this IServerUserHandler self, MessageBase message, Func<IUserInfo, bool> predicate)
		{
			if (self == null)
				throw new ArgumentNullException ("self");
			if (predicate == null)
				throw new ArgumentNullException ("predicate");

			self.Send (message, (c, u) => predicate (u));
		}
	}
}