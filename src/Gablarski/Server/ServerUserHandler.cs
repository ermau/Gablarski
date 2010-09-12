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

		public IUserInfo this[int userId]
		{
			get { return Manager[userId]; }
		}

		public void ApproveRegistration (string username)
		{
			if (username == null)
				throw new ArgumentNullException ("username");
			if (this.context.UserProvider.RegistrationMode != UserRegistrationMode.Approved)
				throw new NotSupportedException();

			this.context.UserProvider.ApproveRegistration (username);
		}

		public void ApproveRegistration (IUserInfo user)
		{
			if (user == null)
				throw new ArgumentNullException ("user");
			if (this.context.UserProvider.RegistrationMode != UserRegistrationMode.PreApproved)
				throw new NotSupportedException();

			lock (this.approvals)
				this.approvals.Add (user);
		}

		public void Disconnect (IConnection connection)
		{
			if (connection == null)
				throw new ArgumentNullException ("connection");

			if (connection.IsConnected)
				Disconnect (ic => ic != connection);
			else
			{
				IUserInfo user = Manager.GetUser (connection);
				Manager.Disconnect (connection);

				if (user != null)
					context.Users.Send (new UserDisconnectedMessage (user.UserId));
			}
		}

		public void Disconnect (Func<IConnection, bool> predicate)
		{
			if (predicate == null)
				throw new ArgumentNullException ("predicate");

			foreach (IConnection c in Manager.GetConnections().Where (predicate))
			{
				IUserInfo user = Manager.GetUser (c);
				if (user != null)
					context.Users.Send (new UserDisconnectedMessage (user.UserId));

				c.Disconnect();
			}

			Manager.Disconnect (predicate);
		}

		public void Move (IUserInfo user, ChannelInfo channel)
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

			foreach (IConnection c in Manager.GetConnections().Where (c => c.IsConnected).Where (predicate))
				c.Send (message);
		}

		public void Send (MessageBase message, Func<IConnection, IUserInfo, bool> predicate)
		{
			if (message == null)
				throw new ArgumentNullException ("message");
			if (predicate == null)
				throw new ArgumentNullException ("predicate");

			var connections = from c in Manager.GetConnections()
			                  let u = Manager.GetUser (c)
			                  where c.IsConnected && u != null && predicate (c, u)
			                  select c;

			foreach (IConnection c in connections)
				c.Send (message);
		}

		public void Disconnect (IUserInfo user, DisconnectionReason reason)
		{
			if (user == null)
				throw new ArgumentNullException ("user");

			IConnection c = Manager.GetConnection (user);
			if (c == null || !c.IsConnected)
				return;

			c.Send (new DisconnectMessage (reason));
			c.DisconnectAsync();

			Send (new UserDisconnectedMessage (user.UserId), ic => ic != c);
		}

		#region IEnumerable<UserInfo> Members

		public IEnumerator<IUserInfo> GetEnumerator()
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
		private readonly HashSet<IUserInfo> approvals = new HashSet<IUserInfo>();

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

			e.Connection.Send (new ServerInfoMessage { ServerInfo = new ServerInfo (context.Settings, context.UserProvider, context.EncryptionParameters) });
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

			IUserInfo info = GetJoiningUserInfo (e.Connection, join);
			if (info == null)
				return;

			LoginResultState result = LoginResultState.Success;

			if (Manager.GetIsNicknameInUse (join.Nickname))
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

			LoginResult result = this.context.UserProvider.Login (login.Username, login.Password);

			if (!this.context.PermissionsProvider.GetPermissions (result.UserId).CheckPermission (PermissionName.Login))
				result.ResultState = LoginResultState.FailedPermissions;

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
			var msg = (RequestUserListMessage) e.Message;

			if (msg.Mode == UserListMode.Current)
			{
				if (!context.GetPermission (PermissionName.RequestChannelList))
				{
					e.Connection.Send (new PermissionDeniedMessage (ClientMessageType.RequestUserList));
					return;
				}

				e.Connection.Send (new UserInfoListMessage (this));
			}
			else
			{
				if (!context.GetPermission (PermissionName.RequestFullUserList))
				{
					e.Connection.Send (new PermissionDeniedMessage (ClientMessageType.RequestUserList));
					return;
				}

				e.Connection.Send (new UserListMessage (context.UserProvider.GetUsers()));
			}
		}

		internal void ChannelChangeMessage (MessageReceivedEventArgs e)
		{
			var change = (ChannelChangeMessage)e.Message;

			ChannelChangeResult resultState = ChannelChangeResult.FailedUnknown;

			ChannelInfo targetChannel = context.ChannelsProvider.GetChannels().FirstOrDefault (c => c.ChannelId == change.TargetChannelId);
			if (targetChannel == null)
				resultState = ChannelChangeResult.FailedUnknownChannel;

			IUserInfo requestingPlayer = context.UserManager.GetUser (e.Connection);

			IUserInfo targetPlayer = context.Users[change.TargetUserId];
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
			{
				e.Connection.Send (new PermissionDeniedMessage (ClientMessageType.RequestMuteUser));
				return;
			}

			IUserInfo user = Manager.FirstOrDefault (u => u.UserId == msg.TargetId);
			if (user == null)
				return;

			if (user.IsMuted == msg.Unmute)
				Manager.ToggleMute (user);
			else
				return;

			this.Send (new UserMutedMessage { UserId = user.UserId, Unmuted = msg.Unmute });
		}

		internal void KickUserMessage (MessageReceivedEventArgs e)
		{
			var msg = (KickUserMessage)e.Message;

			var kicker = context.UserManager.GetUser (e.Connection);
			if (kicker == null)
				return;

			if (msg.FromServer && !context.GetPermission (PermissionName.KickPlayerFromServer, e.Connection))
			{
				e.Connection.Send (new PermissionDeniedMessage (ClientMessageType.KickUser));
				return;
			}

			var user = this.context.Users[msg.UserId];
			if (user == null)
				return;

			if (!msg.FromServer && !context.GetPermission (PermissionName.KickPlayerFromChannel, user.CurrentChannelId, kicker.UserId))
			{
				e.Connection.Send (new PermissionDeniedMessage (ClientMessageType.KickUser));
				return;
			}

			context.Users.Send (new KickedMessage { FromServer = msg.FromServer, UserId = msg.UserId });

			if (msg.FromServer)
				Disconnect (user, DisconnectionReason.Kicked);
			else
				context.Users.Move (user, context.ChannelsProvider.DefaultChannel);
		}

		internal void SetCommentMessage (MessageReceivedEventArgs e)
		{
			var msg = (SetCommentMessage)e.Message;

			IUserInfo user = Manager.GetUser (e.Connection);
			if (user == null || user.Comment == msg.Comment)
				return;

			user = Manager.SetComment (user, msg.Comment);

			this.Send (new UserUpdatedMessage (user));
		}

		internal void SetStatusMessage (MessageReceivedEventArgs e)
		{
			var msg = (SetStatusMessage) e.Message;

			IUserInfo user = Manager.GetUser (e.Connection);
			if (user == null || user.Status == msg.Status)
				return;

			user = Manager.SetStatus (user, msg.Status);

			this.Send (new UserUpdatedMessage (user));
		}

		internal void RegistrationApprovalMessage (MessageReceivedEventArgs e)
		{
			var msg = (RegistrationApprovalMessage)e.Message;

			if (!Manager.GetIsConnected (e.Connection))
				return;

			if (!this.context.GetPermission (PermissionName.ApproveRegistrations, e.Connection))
			{
				e.Connection.Send (new PermissionDeniedMessage (ClientMessageType.RegistrationApproval));
				return;
			}

			if (this.context.UserProvider.RegistrationMode == UserRegistrationMode.Approved)
			{
				var user = this.context.Users[msg.UserId];
				if (user != null)
					ApproveRegistration (user);
			}
			else if (this.context.UserProvider.RegistrationMode == UserRegistrationMode.PreApproved)
			{
				if (msg.Username != null)
					ApproveRegistration (msg.Username);
			}
		}
		
		internal void RegisterMessage (MessageReceivedEventArgs e)
		{
			var msg = (RegisterMessage)e.Message;

			if (!Manager.GetIsConnected (e.Connection))
				return;
			
			if (!this.context.UserProvider.UpdateSupported)
			{
				e.Connection.Send (new RegisterResultMessage { Result = RegisterResult.FailedUnsupported });
				return;
			}

			switch (this.context.UserProvider.RegistrationMode)
			{
				case UserRegistrationMode.Approved:
				case UserRegistrationMode.Normal:
					break;

				case UserRegistrationMode.PreApproved:
					var user = this.context.UserManager.GetUser (e.Connection);
					if (user == null)
						return;

					if (!this.approvals.Remove (user))
					{
						e.Connection.Send (new RegisterResultMessage { Result = RegisterResult.FailedNotApproved });
						return;
					}

					break;

				case UserRegistrationMode.Message:
				case UserRegistrationMode.WebPage:
				case UserRegistrationMode.None:
					e.Connection.Send (new RegisterResultMessage { Result = RegisterResult.FailedUnsupported });
					return;

				default:
					e.Connection.Send (new RegisterResultMessage { Result = RegisterResult.FailedUnknown });
					return;
			}

			var result = this.context.UserProvider.Register (msg.Username, msg.Password);
			e.Connection.Send (new RegisterResultMessage { Result = result });
		}

		internal void SetPermissionsMessage (MessageReceivedEventArgs e)
		{
			var msg = (SetPermissionsMessage)e.Message;

			if (!e.Connection.IsConnected || !context.PermissionsProvider.UpdatedSupported)
				return;

			if (!context.GetPermission (PermissionName.ModifyPermissions, e.Connection))
			{
				e.Connection.Send (new PermissionDeniedMessage (ClientMessageType.SetPermissions));
				return;
			}

			var perms = msg.Permissions.ToList();
			if (perms.Count == 0)
				return;

			context.PermissionsProvider.SetPermissions (msg.UserId, perms);

			IUserInfo target = Manager[msg.UserId];
			if (target == null)
				return;

			IConnection c = Manager.GetConnection (target);
			if (c != null)
				c.Send (new PermissionsMessage (msg.UserId, context.PermissionsProvider.GetPermissions (msg.UserId)));
		}

		private IUserInfo GetJoiningUserInfo (IConnection connection, JoinMessage join)
		{
			if (!Manager.GetIsConnected (connection))
			{
				connection.Send (new JoinResultMessage (LoginResultState.FailedNotConnected, null));
				return null;
			}

			IUserInfo info = this.Manager.GetUser (connection);

			if (info == null)
			{
				if (!this.context.Settings.AllowGuestLogins)
				{
					connection.Send (new JoinResultMessage (LoginResultState.FailedUsername, null));
					return null;
				}

				LoginResult r = this.context.UserProvider.Login (join.Nickname, null);
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
			connection.Send (new ChannelListMessage (this.context.ChannelsProvider.GetChannels(), this.context.ChannelsProvider.DefaultChannel));
			connection.Send (new UserInfoListMessage (Manager));
			connection.Send (new SourceListMessage (this.context.Sources));
		}

		/// <returns><c>true</c> if the nickname is now free, <c>false</c> otherwise.</returns>
		private bool AttemptNicknameRecovery (IUserInfo info, string nickname)
		{
			if (info == null)
				throw new ArgumentNullException ("info");
			if (nickname == null)
				throw new ArgumentNullException ("nickname");

			this.log.DebugFormat ("Attempting nickname recovery for '{0}'", nickname);

			nickname = nickname.ToLower().Trim();

			IUserInfo current = Manager.Single (u => u.Nickname.ToLower().Trim() == nickname);
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