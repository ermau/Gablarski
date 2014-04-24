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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Gablarski.Messages;
using Tempest;

namespace Gablarski.Client
{
	public class ClientUserHandler
		: IClientUserHandler
	{
		protected internal ClientUserHandler (IGablarskiClientContext context)
		{
			if (context == null)
				throw new ArgumentNullException ("context");

			this.context = context;
			this.manager = new ClientUserManager();

			this.context.RegisterMessageHandler<UserInfoListMessage> (OnUserListReceivedMessage);
			this.context.RegisterMessageHandler<UserUpdatedMessage> (OnUserUpdatedMessage);
			this.context.RegisterMessageHandler<UserChangedChannelMessage> (OnUserChangedChannelMessage);
			this.context.RegisterMessageHandler<ChannelChangeResultMessage> (OnChannelChangeResultMessage);
			this.context.RegisterMessageHandler<UserJoinedMessage> (OnUserJoinedMessage);
			this.context.RegisterMessageHandler<UserDisconnectedMessage> (OnUserDisconnectedMessage);
			this.context.RegisterMessageHandler<UserMutedMessage> (OnUserMutedMessage);
			this.context.RegisterMessageHandler<UserKickedMessage> (OnUserKickedMessage);
			this.context.RegisterMessageHandler<JoinResultMessage> (OnJoinResultMessage);
		}

		/// <summary>
		/// An new or updated user list has been received.
		/// </summary>
		public event EventHandler<ReceivedListEventArgs<IUserInfo>> ReceivedUserList;

		/// <summary>
		/// A new user has joined.
		/// </summary>
		public event EventHandler<UserEventArgs> UserJoined;

		/// <summary>
		/// A user has disconnected.
		/// </summary>
		public event EventHandler<UserEventArgs> UserDisconnected;

		/// <summary>
		/// A user was muted.
		/// </summary>
		public event EventHandler<UserMutedEventArgs> UserMuted;

		/// <summary>
		/// A user was ignored.
		/// </summary>
		public event EventHandler<UserMutedEventArgs> UserIgnored;

		/// <summary>
		/// A user's information was updated.
		/// </summary>
		public event EventHandler<UserEventArgs> UserUpdated;

		/// <summary>
		/// A user has changed channels.
		/// </summary>
		public event EventHandler<ChannelChangedEventArgs> UserChangedChannel;

		/// <summary>
		/// A user was kicked.
		/// </summary>
		public event EventHandler<UserKickedEventArgs> UserKicked;

		/// <summary>
		/// Received an unsuccessful result of a change channel request.
		/// </summary>
		public event EventHandler<ReceivedChannelChannelResultEventArgs> ReceivedChannelChangeResult;

		/// <summary>
		/// Gets the current user.
		/// </summary>
		public IUserInfo Current
		{
			get { return this.context.CurrentUser; }
		}

		public int Count
		{
			get { return this.manager.Count; }
		}

		public IUserInfo this[int userId]
		{
			get { return this.manager[userId]; }
		}

		public ILookup<int, IUserInfo> ByChannel
		{
			get { return this.manager.ByChannel; }
		}

		/// <summary>
		/// Requests to move <paramref name="user"/> to <paramref name="targetChannel"/>.
		/// </summary>
		/// <param name="user">The user to move.</param>
		/// <param name="targetChannel">The target channel to move the user to.</param>
		public Task MoveAsync (IUserInfo user, IChannelInfo targetChannel)
		{
			if (user == null)
				throw new ArgumentNullException ("user");
			if (targetChannel == null)
				throw new ArgumentNullException ("targetChannel");

			return this.context.Connection.SendAsync (new ChannelChangeMessage (user.UserId, targetChannel.ChannelId));
		}

		public Task ApproveRegistrationAsync (IUserInfo userInfo)
		{
			if (userInfo == null)
				throw new ArgumentNullException ("userInfo");
			if (context.ServerInfo.RegistrationMode != UserRegistrationMode.PreApproved)
				throw new NotSupportedException();

			return this.context.Connection.SendAsync (new RegistrationApprovalMessage { Approved = true, UserId = userInfo.UserId });
		}

		public Task ApproveRegistrationAsync (string username)
		{
			if (username == null)
				throw new ArgumentNullException ("username");
			if (context.ServerInfo.RegistrationMode != UserRegistrationMode.Approved)
				throw new NotSupportedException();

			return this.context.Connection.SendAsync (new RegistrationApprovalMessage { Approved = true, Username = username });
		}

		public Task RejectRegistrationAsync (string username)
		{
			if (username == null)
				throw new ArgumentNullException ("username");
			if (context.ServerInfo.RegistrationMode != UserRegistrationMode.Approved)
				throw new NotSupportedException();

			return this.context.Connection.SendAsync (new RegistrationApprovalMessage { Approved = false, Username = username });
		}

		public bool GetIsIgnored (IUserInfo user)
		{
			return this.manager.GetIsIgnored (user);
		}

		public bool ToggleIgnore (IUserInfo user)
		{
			if (user == null)
				throw new ArgumentNullException ("user");

			user = this[user.UserId];
			if (user == null)
				return false;

			bool state = this.manager.ToggleIgnore (user);

			OnUserIgnored (new UserMutedEventArgs (user, !state));
			return state;
		}

		public async Task SetMuteAsync (IUserInfo user, bool mute)
		{
			if (user == null)
				throw new ArgumentNullException ("user");

			user = this[user.UserId];
			if (user == null)
				return;

			await this.context.Connection.SendAsync (new RequestMuteUserMessage (user, mute)).ConfigureAwait (false);
		}

		public Task KickAsync (IUser user, bool fromServer)
		{
			if (user == null)
				throw new ArgumentNullException ("user");

			return this.context.Connection.SendAsync (new KickUserMessage (user, fromServer));
		}

		public Task BanAsync (IUserInfo user, TimeSpan banLength)
		{
			if (user == null)
				throw new ArgumentNullException ("user");

			return this.context.Connection.SendAsync (new BanUserMessage {
				BanInfo = new BanInfo (null, user.Username, banLength)
			});
		}

		public void Reset()
		{
			lock (SyncRoot)
			{
				this.manager.Clear();
			}
		}

		public IEnumerator<IUserInfo> GetEnumerator ()
		{
			IEnumerable<IUserInfo> returnUsers;
			lock (SyncRoot)
				 returnUsers = this.manager.Values.ToList();

			return returnUsers.GetEnumerator ();
		}

		IEnumerator IEnumerable.GetEnumerator ()
		{
			return GetEnumerator ();
		}

		public object SyncRoot
		{
			get { return this.manager.SyncRoot; }
		}

		private readonly ClientUserManager manager;
		private readonly IGablarskiClientContext context;

		internal void OnJoinResultMessage (MessageEventArgs<JoinResultMessage> e)
		{
			if (e.Message.Result != LoginResultState.Success)
				return;

			OnUserJoinedMessage (new MessageEventArgs<UserJoinedMessage> (e.Connection,
				new UserJoinedMessage (e.Message.UserInfo)));
		}

		internal void OnUserKickedMessage (MessageEventArgs<UserKickedMessage> e)
		{
			IUserInfo user;
			lock (this.manager.SyncRoot)
				user = this.manager[e.Message.UserId];

			if (e.Message.FromServer)
				OnUserKicked (new UserKickedEventArgs (user));
			else
				OnUserKicked (new UserKickedEventArgs (user, context.Channels[user.CurrentChannelId]));
		}

		internal void OnUserMutedMessage (MessageEventArgs<UserMutedMessage> e)
		{
			var msg = (UserMutedMessage) e.Message;

			bool fire = false;
			IUserInfo user;
			lock (this.manager.SyncRoot)
			{
				user = this.manager[msg.UserId];
				if (user != null && msg.Unmuted == user.IsMuted)
				{
					this.manager.ToggleMute (user);
					fire = true;
				}
			}

			if (fire)
				OnUserMuted (new UserMutedEventArgs (user, msg.Unmuted));
		}

		internal void OnUserListReceivedMessage (MessageEventArgs<UserInfoListMessage> e)
		{
			var msg = (UserInfoListMessage)e.Message;

			IEnumerable<IUserInfo> userlist;
			lock (SyncRoot)
			{
				this.manager.Update (msg.Users);
				userlist = msg.Users.ToList();
			}

			OnReceivedUserList (new ReceivedListEventArgs<IUserInfo> (userlist));
		}

		internal void OnUserUpdatedMessage (MessageEventArgs<UserUpdatedMessage> e)
		{
			var msg = (UserUpdatedMessage) e.Message;

			lock (SyncRoot)
				this.manager.Update (msg.User);

			OnUserUpdated (new UserEventArgs (msg.User));
		}

		internal void OnUserDisconnectedMessage (MessageEventArgs<UserDisconnectedMessage> e)
		{
			var msg = (UserDisconnectedMessage) e.Message;

			IUserInfo user;
			lock (SyncRoot)
			{
				if ((user = this[msg.UserId]) == null)
					return;

				this.manager.Depart (user);
			}

			OnUserDisconnected (new UserEventArgs (user));
		}

		internal void OnUserJoinedMessage (MessageEventArgs<UserJoinedMessage> e)
		{
			var msg = (UserJoinedMessage)e.Message;

			IUserInfo user = new UserInfo (msg.UserInfo);
			
			this.manager.Join (user);

			OnUserJoined (new UserEventArgs (user));
		}

		internal void OnUserChangedChannelMessage (MessageEventArgs<UserChangedChannelMessage> e)
		{
			var msg = (UserChangedChannelMessage)e.Message;

			var channel = this.context.Channels[msg.ChangeInfo.TargetChannelId];
			if (channel == null)
				return;

			var previous = this.context.Channels[msg.ChangeInfo.PreviousChannelId];

			IUserInfo old;
			IUserInfo user;
			IUserInfo movedBy = null;
			lock (SyncRoot)
			{
				if ((old = this[msg.ChangeInfo.TargetUserId]) == null)
					return;

				user = new UserInfo (old.Nickname, old.Phonetic, old.Username, old.UserId, msg.ChangeInfo.TargetChannelId, old.IsMuted);
				this.manager.Update (user);

				if (msg.ChangeInfo.RequestingUserId != 0)
					movedBy = this[msg.ChangeInfo.RequestingUserId];
			}

			OnUserChangedChannnel (new ChannelChangedEventArgs (user, channel, previous, movedBy));
			OnUserUpdated (new UserEventArgs (user));
		}

		internal void OnChannelChangeResultMessage (MessageEventArgs<ChannelChangeResultMessage> e)
		{
			var msg = (ChannelChangeResultMessage)e.Message;

			OnReceivedChannelChangeResult (new ReceivedChannelChannelResultEventArgs (msg.MoveInfo, msg.Result));
		}

		protected virtual void OnUserIgnored (UserMutedEventArgs e)
		{
			var ignored = UserIgnored;
			if (ignored != null)
				ignored (this, e);
		}

		protected virtual void OnUserMuted (UserMutedEventArgs e)
		{
			var muted = UserMuted;
			if (muted != null)
				muted (this, e);
		}

		protected virtual void OnUserUpdated (UserEventArgs e)
		{
			var updated = UserUpdated;
			if (updated != null)
				updated (this, e);
		}

		protected virtual void OnUserDisconnected (UserEventArgs e)
		{
			var disconnected = UserDisconnected;
			if (disconnected != null)
				disconnected (this, e);
		}

		protected virtual void OnReceivedUserList (ReceivedListEventArgs<IUserInfo> e)
		{
			var received = ReceivedUserList;
			if (received != null)
				received (this, e);
		}

		protected virtual void OnUserJoined (UserEventArgs e)
		{
			var result = UserJoined;
			if (result != null)
				result (this, e);
		}

		protected virtual void OnUserChangedChannnel (ChannelChangedEventArgs e)
		{
			var result = UserChangedChannel;
			if (result != null)
				result (this, e);
		}

		protected virtual void OnReceivedChannelChangeResult (ReceivedChannelChannelResultEventArgs e)
		{
			var result = ReceivedChannelChangeResult;
			if (result != null)
				result (this, e);
		}

		private void OnUserKicked (UserKickedEventArgs e)
		{
			var kicked = UserKicked;
			if (kicked != null)
				kicked (this, e);
		}
	}
}