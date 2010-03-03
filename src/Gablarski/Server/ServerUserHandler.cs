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
using System.Net;
using Cadenza;
using Gablarski.Messages;
using log4net;

namespace Gablarski.Server
{
	public class ServerUserHandler
		: IServerUserHandler
	{
		public ServerUserHandler (IServerContext context, IServerUserManager manager)
		{
			this.context = context;
			this.Manager = manager;
			this.log = LogManager.GetLogger (context.Settings.Name.Remove (" ") + ".ServerUserHandler");
		}

		public void Disconnect (IConnection connection)
		{
			if (connection == null)
				throw new ArgumentNullException ("connection");

			UserInfo user = Manager.GetUser (connection);
			if (user != null)
				context.Users.Send (new UserDisconnectedMessage (user.UserId));

			Manager.Disconnect (connection);
		}

		public void Disconnect (Func<IConnection, bool> predicate)
		{
			if (predicate == null)
				throw new ArgumentNullException ("predicate");

			foreach (IConnection c in Manager.GetConnections().Where (predicate))
			{
				UserInfo user = Manager.GetUser (c);
				if (user != null)
					context.Users.Send (new UserDisconnectedMessage (user.UserId));

				c.Disconnect();
			}

			Manager.Disconnect (predicate);
		}

		public void Move (UserInfo user, ChannelInfo channel)
		{
			if (user == null)
				throw new ArgumentNullException ("user");
			if (channel == null)
				throw new ArgumentNullException ("channel");

			int previousChannel = user.CurrentChannelId;

			if (previousChannel == channel.ChannelId)
				return;

			var realUser = this[user.UserId];
			if (realUser == null)
				return;

			var realChannel = context.Channels[channel.ChannelId];
			if (realChannel == null)
				return;

			Manager.Move (realUser, realChannel);
			this.Send (new UserChangedChannelMessage { ChangeInfo =
				new ChannelChangeInfo (user.UserId, channel.ChannelId, previousChannel) });
		}

		public void Send (MessageBase message, Func<IConnection, bool> predicate)
		{
			if (message == null)
				throw new ArgumentNullException ("message");
			if (predicate == null)
				throw new ArgumentNullException ("predicate");

			foreach (IConnection c in Manager.GetConnections().Where (predicate))
				c.Send (message);
		}

		public void Send (MessageBase message, Func<IConnection, UserInfo, bool> predicate)
		{
			if (message == null)
				throw new ArgumentNullException ("message");
			if (predicate == null)
				throw new ArgumentNullException ("predicate");

			var connections = from c in Manager.GetConnections()
			                  let u = Manager.GetUser (c)
			                  where u != null && predicate (c, u)
			                  select c;

			foreach (IConnection c in connections)
				c.Send (message);
		}

		#region IIndexedEnumerable<int,UserInfo> Members

		public UserInfo this[int key]
		{
			get
			{
				return Manager[key];
			}
		}

		#endregion

		#region IEnumerable<UserInfo> Members

		public IEnumerator<UserInfo> GetEnumerator()
		{
			return Manager.GetEnumerator();
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		#endregion

		internal readonly IServerUserManager Manager;
		private readonly ILog log;
		private readonly IServerContext context;

		internal void ConnectMessage (MessageReceivedEventArgs e)
		{
			var msg = (ConnectMessage)e.Message;

			if (msg.ProtocolVersion < this.context.ProtocolVersion)
			{
				e.Connection.Send (new ConnectionRejectedMessage (ConnectionRejectedReason.IncompatibleVersion));
				return;
			}
			
			if (!msg.Host.IsNullOrWhitespace() && msg.Port != 0)
			{
				foreach (var r in this.context.Redirectors)
				{
					IPEndPoint redirect = r.CheckRedirect (msg.Host, msg.Port);
					if (redirect == null)
						continue;
					
					e.Connection.Send (new RedirectMessage { Host = redirect.Address.ToString(), Port = redirect.Port });
					return;
				}
			}

			Manager.Connect (e.Connection);

			e.Connection.Send (new ServerInfoMessage { ServerInfo = new ServerInfo (this.context.Settings) });
		}

		internal void JoinMessage (MessageReceivedEventArgs e)
		{
			var join = (JoinMessage)e.Message;

			if (!e.Connection.IsConnected)
				return;

			if (join.Nickname.IsNullOrWhitespace ())
			{
				e.Connection.Send (new JoinResultMessage (LoginResultState.FailedInvalidNickname, null));
				return;
			}

			if (!String.IsNullOrEmpty (this.context.Settings.ServerPassword) && join.ServerPassword != this.context.Settings.ServerPassword)
			{
				e.Connection.Send (new JoinResultMessage (LoginResultState.FailedServerPassword, null));
				return;
			}

			UserInfo info = GetJoiningUserInfo (e.Connection, join);
			if (info == null)
				return;

			LoginResultState result = LoginResultState.Success;

			if (!this.context.PermissionsProvider.GetPermissions (info.UserId).CheckPermission (PermissionName.Login))
				result = LoginResultState.FailedPermissions;
			else if (Manager.GetIsNicknameInUse (join.Nickname))
			{
				if (!AttemptNicknameRecovery (info, join.Nickname))
					result = LoginResultState.FailedNicknameInUse;
			}

			var msg = new JoinResultMessage (result, info);

			if (result == LoginResultState.Success)
			{
				Manager.Join (e.Connection, info);
				e.Connection.Send (msg);

				if (!Manager.GetIsLoggedIn (e.Connection))
					e.Connection.Send (new PermissionsMessage (info.UserId, this.context.PermissionsProvider.GetPermissions (info.UserId)));

				this.Send (new UserJoinedMessage (info));

				SendInfoMessages (e.Connection);
			}
			else
				e.Connection.Send (msg);
		}

		internal void LoginMessage (MessageReceivedEventArgs e)
		{
			var login = (LoginMessage)e.Message;

			if (login.Username.IsNullOrWhitespace())
			{
				e.Connection.Send (new LoginResultMessage (new LoginResult (0, LoginResultState.FailedUsername)));
				return;
			}

			LoginResult result = this.context.AuthenticationProvider.Login (login.Username, login.Password);

			e.Connection.Send (new LoginResultMessage (result));

			if (result.Succeeded)
			{
				Manager.Login (e.Connection, new UserInfo (login.Username, result.UserId, this.context.ChannelsProvider.DefaultChannel.ChannelId, false));
				e.Connection.Send (new PermissionsMessage (result.UserId, this.context.PermissionsProvider.GetPermissions (result.UserId)));
			}
			
			this.log.DebugFormat ("{0} Login: {1}", login.Username, result.ResultState);
		}

		internal void RequestUserListMessage (MessageReceivedEventArgs e)
		{
			e.Connection.Send (new UserListMessage (this));
		}

		internal void ChannelChangeMessage (MessageReceivedEventArgs e)
		{
			var change = (ChannelChangeMessage)e.Message;

			ChannelChangeResult resultState = ChannelChangeResult.FailedUnknown;

			ChannelInfo targetChannel = context.ChannelsProvider.GetChannels().FirstOrDefault (c => c.ChannelId == change.TargetChannelId);
			if (targetChannel == null)
				resultState = ChannelChangeResult.FailedUnknownChannel;

			UserInfo requestingPlayer = context.UserManager.GetUser (e.Connection);

			UserInfo targetPlayer = context.Users[change.TargetUserId];
			if (targetPlayer == null || targetPlayer.CurrentChannelId == change.TargetChannelId)
				return;

			var changeInfo = new ChannelChangeInfo (change.TargetUserId, change.TargetChannelId, targetPlayer.CurrentChannelId, requestingPlayer.UserId);

			if (resultState == ChannelChangeResult.FailedUnknown)
			{
				if (requestingPlayer.UserId.Equals (change.TargetUserId))
				{
					if (!context.GetPermission (PermissionName.ChangeChannel, e.Connection))
						resultState = ChannelChangeResult.FailedPermissions;
				}
				else if (!context.GetPermission (PermissionName.ChangePlayersChannel, e.Connection))
					resultState = ChannelChangeResult.FailedPermissions;
				
				if (resultState == ChannelChangeResult.FailedUnknown)
				{
					Manager.Move (targetPlayer, targetChannel);
						
					this.Send (new UserChangedChannelMessage { ChangeInfo = changeInfo });
					
					return;
				}
			}

			e.Connection.Send (new ChannelChangeResultMessage (resultState, changeInfo));
		}

		internal void RequestMuteUserMessage (MessageReceivedEventArgs e)
		{
			var msg = (RequestMuteUserMessage)e.Message;

			if (!context.GetPermission (PermissionName.MuteUser, e.Connection))
				return;

			UserInfo user = Manager.FirstOrDefault (u => u.UserId == msg.TargetId);
			if (user == null)
				return;

			if (user.IsMuted == msg.Unmute)
				Manager.ToggleMute (user);
			else
				return;

			this.Send (new MutedMessage { Type = MuteType.User, Target = user.Username, Unmuted = msg.Unmute });
		}

		internal void SetCommentMessage (MessageReceivedEventArgs e)
		{
			var msg = (SetCommentMessage)e.Message;

			UserInfo user = Manager.GetUser (e.Connection);
			if (user == null || user.Comment == msg.Comment)
				return;

			user = Manager.SetComment (user, msg.Comment);

			this.Send (new UserUpdatedMessage (user));
		}

		internal void SetStatusMessage (MessageReceivedEventArgs e)
		{
			var msg = (SetStatusMessage) e.Message;

			UserInfo user = Manager.GetUser (e.Connection);
			if (user == null || user.Status == msg.Status)
				return;

			user = Manager.SetStatus (user, msg.Status);

			this.Send (new UserUpdatedMessage (user));
		}

		private UserInfo GetJoiningUserInfo (IConnection connection, JoinMessage join)
		{
			if (!Manager.GetIsConnected (connection))
			{
				connection.Send (new JoinResultMessage (LoginResultState.FailedNotConnected, null));
				return null;
			}

			UserInfo info = this.Manager.GetUser (connection);

			if (info == null)
			{
				if (!this.context.Settings.AllowGuestLogins)
				{
					connection.Send (new JoinResultMessage (LoginResultState.FailedUsername, null));
					return null;
				}

				LoginResult r = this.context.AuthenticationProvider.Login (join.Nickname, null);
				if (!r.Succeeded)
				{
					connection.Send (new JoinResultMessage (r.ResultState, null));
					return null;
				}

				info = new UserInfo (join.Nickname, join.Phonetic, join.Nickname, r.UserId, this.context.ChannelsProvider.DefaultChannel.ChannelId, false);
			}
			else
				info = new UserInfo (join.Nickname, join.Phonetic, info);

			return info;
		}

		private void SendInfoMessages (IConnection connection)
		{
			connection.Send (new ChannelListMessage (this.context.ChannelsProvider.GetChannels()));
			connection.Send (new UserListMessage (Manager));
			connection.Send (new SourceListMessage (this.context.Sources));
		}

		/// <returns><c>true</c> if the nickname is now free, <c>false</c> otherwise.</returns>
		private bool AttemptNicknameRecovery (UserInfo info, string nickname)
		{
			if (info == null)
				throw new ArgumentNullException ("info");
			if (nickname == null)
				throw new ArgumentNullException ("nickname");

			this.log.DebugFormat ("Attempting nickname recovery for '{0}'", nickname);

			nickname = nickname.ToLower().Trim();

			UserInfo current = Manager.Single (u => u.Nickname.ToLower().Trim() == nickname);
			if (info.UserId == current.UserId)
			{
				this.log.DebugFormat ("Recovery attempt succeeded, disconnecting current '{0}'", current.Nickname);

				Manager.Disconnect (current);
				return true;
			}

			return false;
		}
	}
}