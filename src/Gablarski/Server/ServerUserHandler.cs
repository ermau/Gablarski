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
using System.Net;
using System.Threading.Tasks;
using Cadenza;
using Gablarski.Messages;
using Tempest;

namespace Gablarski.Server
{
	public class ServerUserHandler
		: IServerUserHandler
	{
		public ServerUserHandler (IGablarskiServerContext context)
		{
			if (context == null)
				throw new ArgumentNullException ("context");

			this.context = context;
			Manager = new ServerUserManager();

			this.context.RegisterMessageHandler<ConnectMessage> (OnConnectMessage);
			this.context.RegisterMessageHandler<LoginMessage> (OnLoginMessage);
			this.context.RegisterMessageHandler<JoinMessage> (OnJoinMessage);
			this.context.RegisterMessageHandler<SetCommentMessage> (OnSetCommentMessage);
			this.context.RegisterMessageHandler<SetStatusMessage> (OnSetStatusMessage);
			this.context.RegisterMessageHandler<SetPermissionsMessage> (OnSetPermissionsMessage);
			this.context.RegisterMessageHandler<KickUserMessage> (OnKickUserMessage);
			this.context.RegisterMessageHandler<RegisterMessage> (OnRegisterMessage);
			this.context.RegisterMessageHandler<RegistrationApprovalMessage> (OnRegistrationApprovalMessage);
			this.context.RegisterMessageHandler<BanUserMessage> (OnBanUserMessage);
			this.context.RegisterMessageHandler<RequestMuteUserMessage> (OnRequestMuteUserMessage);
			this.context.RegisterMessageHandler<RequestUserListMessage> (OnRequestUserListMessage);
			this.context.RegisterMessageHandler<ChannelChangeMessage> (OnChannelChangeMessage);
		}

		public int Count
		{
			get { return Manager.Count; }
		}

		public IUserInfo this[int userId]
		{
			get { return Manager[userId]; }
		}

		public IUserInfo this[IConnection connection]
		{
			get { return Manager.GetUser (connection); }
		}

		public IServerConnection this[IUserInfo user]
		{
			get { return (IServerConnection)Manager.GetConnection (user); }
		}

		public IEnumerable<IConnection> Connections
		{
			get { return Manager.GetConnections(); }
		}

		/// <summary>
		/// Gets the connections for joined users.
		/// </summary>
		public IEnumerable<IConnection> UserConnections
		{
			get { return Manager.GetUserConnections(); }
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

		public async Task DisconnectAsync (IConnection connection)
		{
			if (connection == null)
				throw new ArgumentNullException ("connection");

			IUserInfo user = Manager.GetUser (connection);
			Manager.Disconnect (connection);

			List<Task> tasks = new List<Task>();
			if (user != null) {
				foreach (IConnection c in this.context.Connections)
					tasks.Add (c.SendAsync (new UserDisconnectedMessage (user.UserId)));
			}

			tasks.Add (connection.DisconnectAsync());

			await Task.WhenAll (tasks).ConfigureAwait (false);
		}

		public void Move (IUserInfo user, IChannelInfo channel)
		{
			if (user == null)
				throw new ArgumentNullException ("user");
			if (channel == null)
				throw new ArgumentNullException ("channel");

			int previousChannel = user.CurrentChannelId;
			if (previousChannel == channel.ChannelId)
				return;

			IUserInfo realUser = this[user.UserId];
			if (realUser == null)
				return;

			IChannelInfo realChannel = context.Channels[channel.ChannelId];
			if (realChannel == null)
				return;

			var changeInfo = new ChannelChangeInfo (user.UserId, channel.ChannelId, previousChannel);

			int numUsers = context.Users.Count (u => u.CurrentChannelId == channel.ChannelId);
			if (channel.UserLimit != 0 && numUsers >= channel.UserLimit)
				return;

			Manager.Move (realUser, realChannel);

			foreach (var connection in Manager.GetConnections())
				connection.SendAsync (new UserChangedChannelMessage { ChangeInfo = changeInfo });
		}

		public void Move (IConnection mover, IUserInfo user, IChannelInfo channel)
		{
			if (mover == null)
				throw new ArgumentNullException ("mover");
			if (user == null)
				throw new ArgumentNullException ("user");
			if (channel == null)
				throw new ArgumentNullException ("channel");

			IUserInfo movingUser = this.context.Users[mover];
			int moverId = (movingUser != null) ? movingUser.UserId : 0;

			MoveUser (moverId, user.UserId, channel.ChannelId);
		}

		public async Task DisconnectAsync (IUserInfo user, DisconnectionReason reason)
		{
			if (user == null)
				throw new ArgumentNullException ("user");

			IConnection c = Manager.GetConnection (user);
			if (c == null)
				return;

			Manager.Disconnect (c);

			if (c.IsConnected) {
				await c.SendAsync (new DisconnectMessage (reason)).ConfigureAwait (false);
				await c.DisconnectAsync().ConfigureAwait (false);
			}

			List<Task> tasks = new List<Task>();
			foreach (IConnection connection in Manager.GetConnections())
				tasks.Add (connection.SendAsync (new UserDisconnectedMessage (user.UserId)));

			await Task.WhenAll (tasks).ConfigureAwait (false);
		}

		public IEnumerator<IUserInfo> GetEnumerator()
		{
			return Manager.GetEnumerator();
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		internal readonly ServerUserManager Manager;
		private readonly IGablarskiServerContext context;
		private readonly HashSet<IUserInfo> approvals = new HashSet<IUserInfo>();

		internal void OnConnectMessage (MessageEventArgs<ConnectMessage> e)
		{
			var msg = e.Message;
			
			if (!msg.Host.IsNullOrWhitespace() && msg.Port != 0)
			{
				foreach (var r in this.context.Redirectors)
				{
					IPEndPoint redirect = r.CheckRedirect (msg.Host, msg.Port);
					if (redirect == null)
						continue;
					
					e.Connection.SendAsync (new RedirectMessage { Host = redirect.Address.ToString(), Port = redirect.Port });
					return;
				}
			}

			Manager.Connect (e.Connection);

			SendInfoMessages (e.Connection);
		}

		internal void OnJoinMessage (MessageEventArgs<JoinMessage> e)
		{
			var join = e.Message;

			if (!e.Connection.IsConnected)
				return;

			if (join.Nickname.IsNullOrWhitespace ())
			{
				e.Connection.SendResponseAsync (e.Message, new JoinResultMessage (LoginResultState.FailedInvalidNickname, null));
				return;
			}

			if (!String.IsNullOrEmpty (this.context.Settings.ServerPassword) && join.ServerPassword != this.context.Settings.ServerPassword)
			{
				e.Connection.SendResponseAsync (e.Message, new JoinResultMessage (LoginResultState.FailedServerPassword, null));
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
				e.Connection.SendResponseAsync (e.Message, msg);

				if (!Manager.GetIsLoggedIn (e.Connection))
					e.Connection.SendAsync (new PermissionsMessage (info.UserId, this.context.PermissionsProvider.GetPermissions (info.UserId)));

				foreach (IConnection connection in this.context.Connections) {
					if (connection == e.Connection)
						continue;

					connection.SendAsync (new UserJoinedMessage (info));
				}
			}
			else
				e.Connection.SendAsync (msg);
		}

		internal void OnLoginMessage (MessageEventArgs<LoginMessage> e)
		{
			var login = e.Message;

			if (login.Username.IsNullOrWhitespace())
			{
				e.Connection.SendResponseAsync (e.Message, new LoginResultMessage (new LoginResult (0, LoginResultState.FailedUsername)));
				return;
			}

			LoginResult result = this.context.UserProvider.Login (login.Username, login.Password);

			e.Connection.SendResponseAsync (e.Message, new LoginResultMessage (result));

			if (result.Succeeded)
			{
				Manager.Login (e.Connection, new UserInfo (login.Username, result.UserId, this.context.ChannelsProvider.DefaultChannel.ChannelId, false));
				e.Connection.SendAsync (new PermissionsMessage (result.UserId, this.context.PermissionsProvider.GetPermissions (result.UserId)));
			}
		}

		internal void OnRequestUserListMessage (MessageEventArgs<RequestUserListMessage> e)
		{
			var msg = e.Message;

			if (msg.Mode == UserListMode.Current)
			{
				if (!context.GetPermission (PermissionName.RequestChannelList))
				{
					e.Connection.SendAsync (new PermissionDeniedMessage (GablarskiMessageType.RequestUserList));
					return;
				}

				e.Connection.SendAsync (new UserInfoListMessage (this));
			}
			else
			{
				if (!context.GetPermission (PermissionName.RequestFullUserList))
				{
					e.Connection.SendAsync (new PermissionDeniedMessage (GablarskiMessageType.RequestUserList));
					return;
				}

				e.Connection.SendAsync (new UserListMessage (context.UserProvider.GetUsers()));
			}
		}

		internal void OnChannelChangeMessage (MessageEventArgs<ChannelChangeMessage> e)
		{
			var change = (ChannelChangeMessage)e.Message;

			IUserInfo requestingUser = this.context.Users[e.Connection];
			
			MoveUser ((requestingUser != null) ? requestingUser.UserId : 0, change.TargetUserId, change.TargetChannelId);
		}

		internal void OnRequestMuteUserMessage (MessageEventArgs<RequestMuteUserMessage> e)
		{
			var msg = e.Message;

			if (!context.GetPermission (PermissionName.MuteUser, e.Connection))
			{
				e.Connection.SendAsync (new PermissionDeniedMessage (GablarskiMessageType.RequestMuteUser));
				return;
			}

			IUserInfo user = Manager.FirstOrDefault (u => u.UserId == msg.TargetId);
			if (user == null)
				return;

			if (user.IsMuted == msg.Unmute)
				Manager.ToggleMute (user);
			else
				return;

			foreach (IConnection connection in this.context.Connections)
				connection.SendAsync (new UserMutedMessage { UserId = user.UserId, Unmuted = msg.Unmute });
		}

		internal void OnKickUserMessage (MessageEventArgs<KickUserMessage> e)
		{
			var msg = e.Message;

			var kicker = context.Users[e.Connection];
			if (kicker == null)
				return;

			if (msg.FromServer && !context.GetPermission (PermissionName.KickPlayerFromServer, e.Connection))
			{
				e.Connection.SendAsync (new PermissionDeniedMessage (GablarskiMessageType.KickUser));
				return;
			}

			var user = this.context.Users[msg.UserId];
			if (user == null)
				return;

			if (!msg.FromServer && !context.GetPermission (PermissionName.KickPlayerFromChannel, user.CurrentChannelId, kicker.UserId))
			{
				e.Connection.SendAsync (new PermissionDeniedMessage (GablarskiMessageType.KickUser));
				return;
			}

			foreach (IUserInfo u in context.Users)
			{
				IConnection connection = this.context.Users[u];
				if (connection == null)
					continue;

				connection.SendAsync (new UserKickedMessage { FromServer = msg.FromServer, UserId = msg.UserId });
			}

			if (msg.FromServer)
				DisconnectAsync (user, DisconnectionReason.Kicked);
			else
				context.Users.Move (user, context.ChannelsProvider.DefaultChannel);
		}

		internal void OnSetCommentMessage (MessageEventArgs<SetCommentMessage> e)
		{
			var msg = e.Message;

			IUserInfo user = Manager.GetUser (e.Connection);
			if (user == null || user.Comment == msg.Comment)
				return;

			user = Manager.SetComment (user, msg.Comment);

			foreach (IConnection connection in Manager.GetConnections())
				connection.SendAsync (new UserUpdatedMessage (user));
		}

		internal void OnSetStatusMessage (MessageEventArgs<SetStatusMessage> e)
		{
			var msg = e.Message;

			IUserInfo user = Manager.GetUser (e.Connection);
			if (user == null || user.Status == msg.Status)
				return;

			user = Manager.SetStatus (user, msg.Status);

			foreach (IConnection connection in Manager.GetConnections())
				connection.SendAsync (new UserUpdatedMessage (user));
		}

		internal void OnRegistrationApprovalMessage (MessageEventArgs<RegistrationApprovalMessage> e)
		{
			var msg = e.Message;

			if (!Manager.GetIsConnected (e.Connection))
				return;

			if (!this.context.GetPermission (PermissionName.ApproveRegistrations, e.Connection))
			{
				e.Connection.SendAsync (new PermissionDeniedMessage (GablarskiMessageType.RegistrationApproval));
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
		
		internal void OnRegisterMessage (MessageEventArgs<RegisterMessage> e)
		{
			var msg = e.Message;

			if (!Manager.GetIsConnected (e.Connection))
				return;
			
			if (!this.context.UserProvider.UpdateSupported)
			{
				e.Connection.SendResponseAsync (e.Message, new RegisterResultMessage { Result = RegisterResult.FailedUnsupported, Header = new MessageHeader() });
				return;
			}

			switch (this.context.UserProvider.RegistrationMode)
			{
				case UserRegistrationMode.Approved:
				case UserRegistrationMode.Normal:
					break;

				case UserRegistrationMode.PreApproved:
					var user = this.context.Users[e.Connection];
					if (user == null)
						return;

					if (!this.approvals.Remove (user))
					{
						e.Connection.SendResponseAsync (e.Message, new RegisterResultMessage { Result = RegisterResult.FailedNotApproved });
						return;
					}

					break;

				case UserRegistrationMode.Message:
				case UserRegistrationMode.WebPage:
				case UserRegistrationMode.None:
					e.Connection.SendResponseAsync (e.Message, new RegisterResultMessage { Result = RegisterResult.FailedUnsupported });
					return;

				default:
					e.Connection.SendResponseAsync (e.Message, new RegisterResultMessage { Result = RegisterResult.FailedUnknown });
					return;
			}

			var result = this.context.UserProvider.Register (msg.Username, msg.Password);
			e.Connection.SendResponseAsync (e.Message, new RegisterResultMessage { Result = result });
		}

		internal void OnSetPermissionsMessage (MessageEventArgs<SetPermissionsMessage> e)
		{
			var msg = e.Message;

			if (!e.Connection.IsConnected || !context.PermissionsProvider.UpdatedSupported)
				return;

			if (!context.GetPermission (PermissionName.ModifyPermissions, e.Connection))
			{
				e.Connection.SendAsync (new PermissionDeniedMessage (GablarskiMessageType.SetPermissions));
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
				c.SendAsync (new PermissionsMessage (msg.UserId, context.PermissionsProvider.GetPermissions (msg.UserId)));
		}

		internal void OnBanUserMessage (MessageEventArgs<BanUserMessage> e)
		{
			var msg = (BanUserMessage)e.Message;

			if (!Manager.GetIsConnected (e.Connection))
				return;

			if (!context.GetPermission (PermissionName.BanUser, e.Connection))
			{
				e.Connection.SendAsync (new PermissionDeniedMessage (GablarskiMessageType.BanUser));
				return;
			}

			if (msg.Removing)
				context.UserProvider.RemoveBan (msg.BanInfo);
			else
				context.UserProvider.AddBan (msg.BanInfo);
		}

		private IUserInfo GetJoiningUserInfo (IConnection connection, JoinMessage join)
		{
			if (!Manager.GetIsConnected (connection))
			{
				connection.SendAsync (new JoinResultMessage (LoginResultState.FailedNotConnected, null));
				return null;
			}

			IUserInfo info = this.Manager.GetUser (connection);

			if (info == null)
			{
				if (!this.context.Settings.AllowGuestLogins)
				{
					connection.SendAsync (new JoinResultMessage (LoginResultState.FailedUsername, null));
					return null;
				}

				LoginResult r = this.context.UserProvider.Login (join.Nickname, null);
				if (!r.Succeeded)
				{
					connection.SendAsync (new JoinResultMessage (r.ResultState, null));
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
			connection.SendAsync (new ServerInfoMessage { ServerInfo = new ServerInfo (context.Settings, context.UserProvider) });
			connection.SendAsync (new ChannelListMessage (this.context.ChannelsProvider.GetChannels(), this.context.ChannelsProvider.DefaultChannel));
			connection.SendAsync (new UserInfoListMessage (Manager));
			connection.SendAsync (new SourceListMessage (this.context.Sources));
		}

		/// <returns><c>true</c> if the nickname is now free, <c>false</c> otherwise.</returns>
		private bool AttemptNicknameRecovery (IUserInfo info, string nickname)
		{
			if (info == null)
				throw new ArgumentNullException ("info");
			if (nickname == null)
				throw new ArgumentNullException ("nickname");

			nickname = nickname.ToLower().Trim();

			IUserInfo current = Manager.Where (u => u.Nickname != null).Single (u => u.Nickname.ToLower().Trim() == nickname);
			if (info.IsRegistered && info.UserId == current.UserId) {
				Manager.Disconnect (current);
				return true;
			}

			return false;
		}

		private void MoveUser (int moverId, int userId, int channelId)
		{
			IConnection requester = null;
			IUserInfo requestingUser = (moverId == 0) ? null : this.context.Users[moverId];
			if (requestingUser != null)
				requester = this.context.Users[requestingUser];
			
			IUserInfo user = this.context.Users[userId];
			if (user == null)
				return;

			ChannelChangeInfo info = (requestingUser != null)
			                         	? new ChannelChangeInfo (userId, channelId, user.CurrentChannelId, requestingUser.UserId)
			                         	: new ChannelChangeInfo (userId, channelId, user.CurrentChannelId);

			IChannelInfo channel = this.context.Channels[channelId];
			if (channel == null)
			{
				if (requester != null)
					requester.SendAsync (new ChannelChangeResultMessage (ChannelChangeResult.FailedUnknownChannel, info));

				return;
			}

			if (!user.Equals (requestingUser))
			{
				if (!this.context.GetPermission (PermissionName.ChangePlayersChannel, channel.ChannelId, moverId))
				{
					if (requester != null)
						requester.SendAsync (new ChannelChangeResultMessage (ChannelChangeResult.FailedPermissions, info));

					return;
				}
			}
			else if (!this.context.GetPermission (PermissionName.ChangeChannel, channel.ChannelId, user.UserId))
			{
				if (requester != null)
					requester.SendAsync (new ChannelChangeResultMessage (ChannelChangeResult.FailedPermissions, info));

				return;
			}

			if (channel.UserLimit != 0 && channel.UserLimit <= context.Users.Count (u => u.CurrentChannelId == channel.ChannelId))
			{
				if (requester != null)
					requester.SendAsync (new ChannelChangeResultMessage (ChannelChangeResult.FailedFull, info));

				return;
			}

			Manager.Move (user, channel);

			foreach (IConnection connection in Manager.GetConnections())
				connection.SendAsync (new UserChangedChannelMessage { ChangeInfo = info });
		}
	}
}