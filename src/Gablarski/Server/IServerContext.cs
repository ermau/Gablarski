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

namespace Gablarski.Server
{
	public interface IServerContext
	{
		object SyncRoot { get; }

		/// <summary>
		/// Gets the authentication provider for the server.
		/// </summary>
		IUserProvider UserProvider { get; }

		/// <summary>
		/// Gets the permissions provider for the server.
		/// </summary>
		IPermissionsProvider PermissionsProvider { get; }

		/// <summary>
		/// Gets the channel provider for the server.
		/// </summary>
		IChannelProvider ChannelsProvider { get; }

		/// <summary>
		/// Gets the protocol version of the server.
		/// </summary>
		int ProtocolVersion { get; }

		/// <summary>
		/// Gets the connection handler for the server.
		/// </summary>
		IConnectionHandler Connections { get; }

		/// <summary>
		/// Gets the user handler for the server.
		/// </summary>
		IServerUserHandler Users { get; }

		/// <summary>
		/// Gets the user manager for the server.
		/// </summary>
		IServerUserManager UserManager { get; }

		/// <summary>
		/// Gets the source handler for the server.
		/// </summary>
		IServerSourceHandler Sources { get; }

		/// <summary>
		/// Gets the channel handler for the server.
		/// </summary>
		IServerChannelHandler Channels { get; }

		/// <summary>
		/// Gets the redirectors for this server.
		/// </summary>
		IEnumerable<IRedirector> Redirectors { get; }

		/// <summary>
		/// Gets the public encryption parameters for this server.
		/// </summary>
		PublicRSAParameters EncryptionParameters { get; }

		/// <summary>
		/// Gets the settings for this server.
		/// </summary>
		ServerSettings Settings { get; }
	}

	public static class ServerContextExtensions
	{
		public static bool GetPermission (this IServerContext self, PermissionName name, IConnection connection)
		{
			if (self == null)
				throw new ArgumentNullException ("self");
			if (connection == null)
				return self.GetPermission (name);

			UserInfo user = self.UserManager.GetUser (connection);
			if (user == null)
				return self.GetPermission (name);

			return self.GetPermission (name, user.CurrentChannelId, user.UserId);
		}

		public static bool GetPermission (this IServerContext self, PermissionName name, UserInfo user)
		{
			if (user == null)
				throw new ArgumentNullException ("user");

			return GetPermission (self, name, user.CurrentChannelId, user.UserId);
		}

		public static bool GetPermission (this IServerContext self, PermissionName name, ChannelInfo channel, IConnection connection)
		{
			if (self == null)
				throw new ArgumentNullException ("self");
			if (channel == null)
				throw new ArgumentNullException ("channel");
			if (connection == null)
				throw new ArgumentNullException ("connection");

			UserInfo user = self.UserManager.GetUser (connection);
			if (user == null)
				return self.GetPermission (name);

			return GetPermission (self, name, channel.ChannelId, user.UserId);
		}

		public static bool GetPermission (this IServerContext self, PermissionName name, ChannelInfo channel, UserInfo user)
		{
			if (channel == null)
				throw new ArgumentNullException ("channel");
			if (user == null)
				throw new ArgumentNullException ("user");

			return GetPermission (self, name, channel.ChannelId, user.UserId);
		}

		public static bool GetPermission (this IServerContext self, PermissionName name, int channelId, int userId)
		{
			if (self == null)
				throw new ArgumentNullException ("self");

			return self.PermissionsProvider.GetPermissions (userId).CheckPermission (channelId, name);
		}

		public static bool GetPermission (this IServerContext self, PermissionName name)
		{
			if (self == null)
				throw new ArgumentNullException ("self");

			return self.PermissionsProvider.GetPermissions (0).CheckPermission (name);
		}
	}
}