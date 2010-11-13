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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cadenza.Collections;
using Gablarski.Messages;

namespace Gablarski.Client
{
	public class ClientUserHandler
		: IClientUserHandler
	{
		protected internal ClientUserHandler (IClientContext context, IClientUserManager manager)
		{
			if (context == null)
				throw new ArgumentNullException ("context");
			if (manager == null)
				throw new ArgumentNullException ("manager");

			this.context = context;
			this.manager = manager;

			this.context.RegisterMessageHandler (ServerMessageType.UserInfoList, OnUserListReceivedMessage);
			this.context.RegisterMessageHandler (ServerMessageType.UserUpdated, OnUserUpdatedMessage);
			this.context.RegisterMessageHandler (ServerMessageType.UserChangedChannel, OnUserChangedChannelMessage);
			this.context.RegisterMessageHandler (ServerMessageType.ChangeChannelResult, OnChannelChangeResultMessage);
			this.context.RegisterMessageHandler (ServerMessageType.UserJoined, OnUserJoinedMessage);
			this.context.RegisterMessageHandler (ServerMessageType.UserDisconnected, OnUserDisconnectedMessage);
			this.context.RegisterMessageHandler (ServerMessageType.UserMuted, OnUserMutedMessage);
			this.context.RegisterMessageHandler (ServerMessageType.UserKicked, OnUserKickedMessage);
		}

		#region Events
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
		/// A user was kicked from your current channel.
		/// </summary>
		public event EventHandler<UserEventArgs> UserKickedFromChannel;

		/// <summary>
		/// A user was kicked from the server.
		/// </summary>
		public event EventHandler<UserEventArgs> UserKickedFromServer;

		/// <summary>
		/// Received an unsuccessful result of a change channel request.
		/// </summary>
		public event EventHandler<ReceivedChannelChannelResultEventArgs> ReceivedChannelChangeResult;
		#endregion

		/// <summary>
		/// Gets the current user.
		/// </summary>
		public IUserInfo Current
		{
			get { return this.context.CurrentUser; }
		}

		/// <summary>
		/// Requests to move <paramref name="user"/> to <paramref name="targetChannel"/>.
		/// </summary>
		/// <param name="user">The user to move.</param>
		/// <param name="targetChannel">The target channel to move the user to.</param>
		public void Move (IUserInfo user, IChannelInfo targetChannel)
		{
			if (user == null)
				throw new ArgumentNullException ("user");
			if (targetChannel == null)
				throw new ArgumentNullException ("targetChannel");

			this.context.Connection.Send (new ChannelChangeMessage (user.UserId, targetChannel.ChannelId));
		}

		public void ApproveRegistration (IUserInfo userInfo)
		{
			if (userInfo == null)
				throw new ArgumentNullException ("userInfo");
			if (context.ServerInfo.RegistrationMode != UserRegistrationMode.PreApproved)
				throw new NotSupportedException();

			this.context.Connection.Send (new RegistrationApprovalMessage { Approved = true, UserId = userInfo.UserId });
		}

		public void ApproveRegistration (string username)
		{
			if (username == null)
				throw new ArgumentNullException ("username");
			if (context.ServerInfo.RegistrationMode != UserRegistrationMode.Approved)
				throw new NotSupportedException();

			this.context.Connection.Send (new RegistrationApprovalMessage { Approved = true, Username = username });
		}

		public void RejectRegistration (string username)
		{
			if (username == null)
				throw new ArgumentNullException ("username");
			if (context.ServerInfo.RegistrationMode != UserRegistrationMode.Approved)
				throw new NotSupportedException();

			this.context.Connection.Send (new RegistrationApprovalMessage { Approved = false, Username = username });
		}

		public bool GetIsIgnored (IUserInfo user)
		{
			return this.manager.GetIsIgnored (user);
		}

		public bool ToggleIgnore (IUserInfo user)
		{
			if (user == null)
				throw new ArgumentNullException ("user");

			bool state = this.manager.ToggleIgnore (user);

			OnUserIgnored (new UserMutedEventArgs (user, !state));
			return state;
		}

		public void ToggleMute (IUserInfo user)
		{
			if (user == null)
				throw new ArgumentNullException ("user");

			context.Connection.Send (new RequestMuteUserMessage (user, !user.IsMuted));
		}

		public void Kick (IUser user, bool fromServer)
		{
			if (user == null)
				throw new ArgumentNullException ("user");

			this.context.Connection.Send (new KickUserMessage (user, fromServer));
		}

		public IEnumerable<IUserInfo> GetUsersInChannel (int channelId)
		{
			lock (SyncRoot)
			{
				return this.channels[channelId].ToList();
			}
		}

		

		public IUserInfo this[int userId]
		{
			get { return this.manager[userId]; }
		}

		public void Reset()
		{
			lock (SyncRoot)
			{
				this.manager.Clear();
				channels.Clear();
			}
		}

		public IEnumerator<IUserInfo> GetEnumerator ()
		{
			IEnumerable<IUserInfo> returnUsers;
			lock (SyncRoot)
			{
				 returnUsers = this.manager.ToList();
			}

			return returnUsers.GetEnumerator ();
		}

		IEnumerator IEnumerable.GetEnumerator ()
		{
			return GetEnumerator ();
		}

		protected object SyncRoot
		{
			get { return this.manager.SyncRoot; }
		}

		private readonly IClientUserManager manager;
		private readonly MutableLookup<int, IUserInfo> channels = new MutableLookup<int, IUserInfo> ();
		private readonly IClientContext context;

		#region Message handlers
		internal void OnUserKickedMessage (MessageReceivedEventArgs e)
		{
			var msg = (UserKickedMessage) e.Message;

			IUserInfo user;
			lock (this.manager.SyncRoot)
				user = this.manager[msg.UserId];

			if (msg.FromServer)
				OnUserKickedFromServer (new UserEventArgs (user));
			else
				OnUserKickedFromChannel (new UserEventArgs (user));
		}

		internal void OnUserMutedMessage (MessageReceivedEventArgs e)
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

		internal void OnUserListReceivedMessage (MessageReceivedEventArgs e)
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

		internal void OnUserUpdatedMessage (MessageReceivedEventArgs e)
		{
			var msg = (UserUpdatedMessage) e.Message;

			lock (SyncRoot)
				this.manager.Update (msg.User);

			OnUserUpdated (new UserEventArgs (msg.User));
		}

		internal void OnUserDisconnectedMessage (MessageReceivedEventArgs e)
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

		internal void OnUserJoinedMessage (MessageReceivedEventArgs e)
		{
			var msg = (UserJoinedMessage)e.Message;

			IUserInfo user = new UserInfo (msg.UserInfo);
			
			this.manager.Join (user);

			OnUserJoined (new UserEventArgs (user));
		}

		internal void OnUserChangedChannelMessage (MessageReceivedEventArgs e)
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

		internal void OnChannelChangeResultMessage (MessageReceivedEventArgs e)
		{
			var msg = (ChannelChangeResultMessage)e.Message;

			OnReceivedChannelChangeResult (new ReceivedChannelChannelResultEventArgs (msg.MoveInfo, msg.Result));
		}
		#endregion

		#region Event Invokers
		protected virtual void OnUserIgnored (UserMutedEventArgs e)
		{
			var ignored = this.UserIgnored;
			if (ignored != null)
				ignored (this, e);
		}

		protected virtual void OnUserMuted (UserMutedEventArgs e)
		{
			var muted = this.UserMuted;
			if (muted != null)
				muted (this, e);
		}

		protected virtual void OnUserUpdated (UserEventArgs e)
		{
			var updated = this.UserUpdated;
			if (updated != null)
				updated (this, e);
		}

		protected virtual void OnUserDisconnected (UserEventArgs e)
		{
			var disconnected = this.UserDisconnected;
			if (disconnected != null)
				disconnected (this, e);
		}

		protected virtual void OnReceivedUserList (ReceivedListEventArgs<IUserInfo> e)
		{
			var received = this.ReceivedUserList;
			if (received != null)
				received (this, e);
		}

		protected virtual void OnUserJoined (UserEventArgs e)
		{
			var result = this.UserJoined;
			if (result != null)
				result (this, e);
		}

		protected virtual void OnUserChangedChannnel (ChannelChangedEventArgs e)
		{
			var result = this.UserChangedChannel;
			if (result != null)
				result (this, e);
		}

		protected virtual void OnReceivedChannelChangeResult (ReceivedChannelChannelResultEventArgs e)
		{
			var result = this.ReceivedChannelChangeResult;
			if (result != null)
				result (this, e);
		}

		private void OnUserKickedFromServer (UserEventArgs e)
		{
			var kicked = UserKickedFromServer;
			if (kicked != null)
				kicked (this, e);
		}

		private void OnUserKickedFromChannel (UserEventArgs e)
		{
			var kicked = UserKickedFromChannel;
			if (kicked != null)
				kicked (this, e);
		}
		#endregion
	}

	#region Event Args
	public class UserMutedEventArgs
		: UserEventArgs
	{
		public UserMutedEventArgs (IUserInfo userInfo, bool unmuted)
			: base (userInfo)
		{
			this.Unmuted = unmuted;
		}

		public bool Unmuted { get; set; }
	}

	public class UserEventArgs
		: EventArgs
	{
		public UserEventArgs (IUserInfo userInfo)
		{
			if (userInfo == null)
				throw new ArgumentNullException ("userInfo");

			this.User = userInfo;
		}

		/// <summary>
		/// Gets the user target of the event.
		/// </summary>
		public IUserInfo User
		{
			get;
			private set;
		}
	}

	public class ChannelChangedEventArgs
		: UserEventArgs
	{
		public ChannelChangedEventArgs (IUserInfo target, IChannelInfo targetChannel, IChannelInfo previousChannel, IUserInfo movedBy)
			: base (target)
		{
			if (targetChannel == null)
				throw new ArgumentNullException ("targetChannel");

			this.TargetChannel = targetChannel;
			this.MovedBy = movedBy;
			this.PreviousChannel = previousChannel;
		}

		/// <summary>
		/// Gets the channel the user is being moved to.
		/// </summary>
		public IChannelInfo TargetChannel
		{
			get; private set;
		}

		/// <summary>
		/// Gets the channel the user is being moved from.
		/// </summary>
		public IChannelInfo PreviousChannel
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets the user the target user was moved by
		/// </summary>
		public IUserInfo MovedBy
		{
			get; private set;
		}
	}

	public class ReceivedChannelChannelResultEventArgs
		: EventArgs
	{
		public ReceivedChannelChannelResultEventArgs (ChannelChangeInfo moveInfo, ChannelChangeResult result)
		{
			if (moveInfo == null)
				throw new ArgumentNullException ("moveInfo");

			this.MoveInfo = moveInfo;
			this.Result = result;
		}

		/// <summary>
		/// Gets information about the move this result is for.
		/// </summary>
		public ChannelChangeInfo MoveInfo
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets the result of the change channel request.
		/// </summary>
		public ChannelChangeResult Result
		{
			get;
			private set;
		}
	}
	#endregion
}