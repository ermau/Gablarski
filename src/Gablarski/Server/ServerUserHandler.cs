// Copyright (c) 2011-2013, Eric Maupin
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
using log4net;
using Tempest;

namespace Gablarski.Server
{
	public class ServerUserHandler
		: IServerUserHandler
	{
		public ServerUserHandler (IGablarskiServerContext context, IServerUserManager manager)
		{
			this.context = context;
			this.Manager = manager;
			this.log = LogManager.GetLogger (context.Settings.Name.Remove (" ") + ".ServerUserHandler");

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

			if (connection.IsConnected) {
				await DisconnectAsync (ic => ic != connection).ConfigureAwait (false);
				return;
			}

			IUserInfo user = this.Manager.GetUser (connection);
			this.Manager.Disconnect (connection);

			if (user != null)
				await this.context.Users.SendAsync (new UserDisconnectedMessage (user.UserId)).ConfigureAwait (false);
		}

		public async Task DisconnectAsync (Func<IConnection, bool> predicate)
		{
			if (predicate == null)
				throw new ArgumentNullException ("predicate");

			List<Task> tasks = new List<Task>();
			foreach (IConnection c in Manager.GetConnections().Where (predicate))
			{
				IUserInfo user = Manager.GetUser (c);
				if (user != null)
					tasks.Add (context.Users.SendAsync (new UserDisconnectedMessage (user.UserId)));

				tasks.Add (c.DisconnectAsync());
			}

			await Task.WhenAll (tasks).ConfigureAwait (false);

			Manager.Disconnect (predicate);
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
			this.SendAsync (new UserChangedChannelMessage { ChangeInfo = changeInfo });
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

		public Task SendAsync (Message message, Func<IConnection, bool> predicate)
		{
			if (message == null)
				throw new ArgumentNullException ("message");
			if (predicate == null)
				throw new ArgumentNullException ("predicate");

			List<Task> tasks = new List<Task>();
			foreach (IConnection c in Manager.GetConnections().Where (c => c.IsConnected).Where (predicate))
				tasks.Add (c.SendAsync (message));

		    return Task.WhenAll (tasks);
		}

		public Task SendAsync (Message message, Func<IConnection, IUserInfo, bool> predicate)
		{
			if (message == null)
				throw new ArgumentNullException ("message");
			if (predicate == null)
				throw new ArgumentNullException ("predicate");

			var connections = from c in Manager.GetConnections()
			                  let u = Manager.GetUser (c)
			                  where c.IsConnected && u != null && predicate (c, u)
			                  select c;

			return Task.WhenAll (connections.Select (c => c.SendAsync (message)));
		}

		public async Task DisconnectAsync (IUserInfo user, DisconnectionReason reason)
		{
			if (user == null)
				throw new ArgumentNullException ("user");

			IConnection c = Manager.GetConnection (user);
			if (c == null)
				return;

			if (c.IsConnected) {
				await c.SendAsync (new DisconnectMessage (reason)).ConfigureAwait (false);
				await c.DisconnectAsync().ConfigureAwait (false);
			}

			await SendAsync (new UserDisconnectedMessage (user.UserId), ic => ic != c).ConfigureAwait (false);
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
		private readonly IGablarskiServerContext context;
		private readonly HashSet<IUserInfo> approvals = new HashSet<IUserInfo>();

		internal void OnConnectMessage (MessageEventArgs<ConnectMessage> e)
		{
			var msg = (ConnectMessage)e.Message;

			//if (msg.ProtocolVersion < this.context.ProtocolVersion)
			//{
			//    e.Connection.Send (new ConnectionRejectedMessage (ConnectionRejectedReason.IncompatibleVersion));
			//    return;
			//}
			
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
			var join = (JoinMessage)e.Message;

			if (!e.Connection.IsConnected)
				return;

			if (join.Nickname.IsNullOrWhitespace ())
			{
				e.Connection.SendAsync (new JoinResultMessage (LoginResultState.FailedInvalidNickname, null));
				return;
			}

			if (!String.IsNullOrEmpty (this.context.Settings.ServerPassword) && join.ServerPassword != this.context.Settings.ServerPassword)
			{
				e.Connection.SendAsync (new JoinResultMessage (LoginResultState.FailedServerPassword, null));
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
				e.Connection.SendAsync (msg);

				if (!Manager.GetIsLoggedIn (e.Connection))
					e.Connection.SendAsync (new PermissionsMessage (info.UserId, this.context.PermissionsProvider.GetPermissions (info.UserId)));

				this.SendAsync (new UserJoinedMessage (info));

				//SendInfoMessages (e.Connection);
			}
			else
				e.Connection.SendAsync (msg);
		}

		internal void OnLoginMessage (MessageEventArgs<LoginMessage> e)
		{
			var login = (LoginMessage)e.Message;

			if (login.Username.IsNullOrWhitespace())
			{
				e.Connection.SendAsync (new LoginResultMessage (new LoginResult (0, LoginResultState.FailedUsername)));
				return;
			}

			LoginResult result = this.context.UserProvider.Login (login.Username, login.Password);

			e.Connection.SendAsync (new LoginResultMessage (result));

			if (result.Succeeded)
			{
				Manager.Login (e.Connection, new UserInfo (login.Username, result.UserId, this.context.ChannelsProvider.DefaultChannel.ChannelId, false));
				e.Connection.SendAsync (new PermissionsMessage (result.UserId, this.context.PermissionsProvider.GetPermissions (result.UserId)));
			}
			
			this.log.DebugFormat ("{0} Login: {1}", login.Username, result.ResultState);
		}

		internal void OnRequestUserListMessage (MessageEventArgs<RequestUserListMessage> e)
		{
			var msg = (RequestUserListMessage) e.Message;

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
			var msg = (RequestMuteUserMessage)e.Message;

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

			this.SendAsync (new UserMutedMessage { UserId = user.UserId, Unmuted = msg.Unmute });
		}

		internal void OnKickUserMessage (MessageEventArgs<KickUserMessage> e)
		{
			var msg = (KickUserMessage)e.Message;

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

			context.Users.SendAsync (new UserKickedMessage { FromServer = msg.FromServer, UserId = msg.UserId });

			if (msg.FromServer)
				DisconnectAsync (user, DisconnectionReason.Kicked);
			else
				context.Users.Move (user, context.ChannelsProvider.DefaultChannel);
		}

		internal void OnSetCommentMessage (MessageEventArgs<SetCommentMessage> e)
		{
			var msg = (SetCommentMessage)e.Message;

			IUserInfo user = Manager.GetUser (e.Connection);
			if (user == null || user.Comment == msg.Comment)
				return;

			user = Manager.SetComment (user, msg.Comment);

			this.SendAsync (new UserUpdatedMessage (user));
		}

		internal void OnSetStatusMessage (MessageEventArgs<SetStatusMessage> e)
		{
			var msg = (SetStatusMessage) e.Message;

			IUserInfo user = Manager.GetUser (e.Connection);
			if (user == null || user.Status == msg.Status)
				return;

			user = Manager.SetStatus (user, msg.Status);

			this.SendAsync (new UserUpdatedMessage (user));
		}

		internal void OnRegistrationApprovalMessage (MessageEventArgs<RegistrationApprovalMessage> e)
		{
			var msg = (RegistrationApprovalMessage)e.Message;

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
			var msg = (RegisterMessage)e.Message;

			if (!Manager.GetIsConnected (e.Connection))
				return;
			
			if (!this.context.UserProvider.UpdateSupported)
			{
				e.Connection.SendAsync (new RegisterResultMessage { Result = RegisterResult.FailedUnsupported });
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
						e.Connection.SendAsync (new RegisterResultMessage { Result = RegisterResult.FailedNotApproved });
						return;
					}

					break;

				case UserRegistrationMode.Message:
				case UserRegistrationMode.WebPage:
				case UserRegistrationMode.None:
					e.Connection.SendAsync (new RegisterResultMessage { Result = RegisterResult.FailedUnsupported });
					return;

				default:
					e.Connection.SendAsync (new RegisterResultMessage { Result = RegisterResult.FailedUnknown });
					return;
			}

			var result = this.context.UserProvider.Register (msg.Username, msg.Password);
			e.Connection.SendAsync (new RegisterResultMessage { Result = result });
		}

		internal void OnSetPermissionsMessage (MessageEventArgs<SetPermissionsMessage> e)
		{
			var msg = (SetPermissionsMessage)e.Message;

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

			this.log.DebugFormat ("Attempting nickname recovery for '{0}'", nickname);

			nickname = nickname.ToLower().Trim();

			IUserInfo current = Manager.Single (u => u.Nickname.ToLower().Trim() == nickname);
			if (info.IsRegistered && info.UserId == current.UserId)
			{
				this.log.DebugFormat ("Recovery attempt succeeded, disconnecting current '{0}'", current.Nickname);

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
			this.SendAsync (new UserChangedChannelMessage { ChangeInfo = info });
		}
	}
}