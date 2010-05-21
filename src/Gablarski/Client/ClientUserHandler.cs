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
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using Cadenza.Collections;
using Gablarski.Messages;
using System.Threading;

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
			this.context.RegisterMessageHandler (ServerMessageType.UserLoggedIn, OnUserJoinedMessage);
			this.context.RegisterMessageHandler (ServerMessageType.UserDisconnected, OnUserDisconnectedMessage);
			this.context.RegisterMessageHandler (ServerMessageType.UserMuted, OnUserMutedMessage);
		}

		#region Events
		/// <summary>
		/// An new or updated user list has been received.
		/// </summary>
		public event EventHandler<ReceivedListEventArgs<UserInfo>> ReceivedUserList;

		/// <summary>
		/// A new user has joined.
		/// </summary>
		public event EventHandler<UserEventArgs> UserJoined;

		/// <summary>
		/// A user has disconnected.
		/// </summary>
		public event EventHandler<UserEventArgs> UserDisconnected;

		/// <summary>
		/// A user was muted or ignored.
		/// </summary>
		public event EventHandler<UserMutedEventArgs> UserMuted;

		/// <summary>
		/// A user's information was updated.
		/// </summary>
		public event EventHandler<UserEventArgs> UserUpdated;

		/// <summary>
		/// A user has changed channels.
		/// </summary>
		public event EventHandler<ChannelChangedEventArgs> UserChangedChannel;

		/// <summary>
		/// Received an unsucessful result of a change channel request.
		/// </summary>
		public event EventHandler<ReceivedChannelChannelResultEventArgs> ReceivedChannelChangeResult;
		#endregion

		/// <summary>
		/// Gets the current user.
		/// </summary>
		public CurrentUser Current
		{
			get { return this.context.CurrentUser; }
		}

		/// <summary>
		/// Requests to move <paramref name="user"/> to <paramref name="targetChannel"/>.
		/// </summary>
		/// <param name="user">The user to move.</param>
		/// <param name="targetChannel">The target channel to move the user to.</param>
		public void Move (UserInfo user, ChannelInfo targetChannel)
		{
			if (user == null)
				throw new ArgumentNullException ("user");
			if (targetChannel == null)
				throw new ArgumentNullException ("targetChannel");

			this.context.Connection.Send (new ChannelChangeMessage (user.UserId, targetChannel.ChannelId));
		}

		public bool GetIsIgnored (UserInfo user)
		{
			return this.manager.GetIsIgnored (user);
		}

		public bool ToggleIgnore (UserInfo user)
		{
			return this.manager.ToggleIgnore (user);
		}

		public void ToggleMute (UserInfo user)
		{
			if (user == null)
				throw new ArgumentNullException ("user");

			context.Connection.Send (new RequestMuteUserMessage (user, !user.IsMuted));
		}

		public IEnumerable<UserInfo> GetUsersInChannel (int channelId)
		{
			lock (SyncRoot)
			{
				return this.channels[channelId].ToList();
			}
		}

		public UserInfo this[int userId]
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

		public IEnumerator<UserInfo> GetEnumerator ()
		{
			IEnumerable<UserInfo> returnUsers;
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
		private readonly MutableLookup<int, UserInfo> channels = new MutableLookup<int, UserInfo> ();
		private readonly IClientContext context;

		#region Message handlers
		internal void OnUserMutedMessage (MessageReceivedEventArgs e)
		{
			var msg = (UserMutedMessage) e.Message;

			bool fire = false;
			UserInfo user;
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

			IEnumerable<UserInfo> userlist;
			lock (SyncRoot)
			{
				this.manager.Update (msg.Users);
				userlist = msg.Users.ToList();
			}

			OnReceivedUserList (new ReceivedListEventArgs<UserInfo> (userlist));
		}

		internal void OnUserUpdatedMessage (MessageReceivedEventArgs e)
		{
			var msg = (UserUpdatedMessage) e.Message;

			lock (SyncRoot)
			{
				this.manager.Update (msg.User);

				if (msg.User.Equals (context.CurrentUser))
				{
					context.CurrentUser.Comment = msg.User.Comment;
					context.CurrentUser.Status = msg.User.Status;
				}
			}

			OnUserUpdated (new UserEventArgs (msg.User));
		}

		internal void OnUserDisconnectedMessage (MessageReceivedEventArgs e)
		{
			var msg = (UserDisconnectedMessage) e.Message;

			UserInfo user;
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

			UserInfo user = new UserInfo (msg.UserInfo);
			
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

			UserInfo old;
			UserInfo user;
			UserInfo movedBy = null;
			lock (SyncRoot)
			{
				if ((old = this[msg.ChangeInfo.TargetUserId]) == null)
					return;

				user = new UserInfo (old.Nickname, old.Phonetic, old.Username, old.UserId, msg.ChangeInfo.TargetChannelId, old.IsMuted);
				this.manager.Update (user);
				
				if (user.Equals (context.CurrentUser))
					context.CurrentUser.CurrentChannelId = msg.ChangeInfo.TargetChannelId;

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

		protected virtual void OnReceivedUserList (ReceivedListEventArgs<UserInfo> e)
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
		#endregion
	}

	#region Event Args
	public class UserMutedEventArgs
		: UserEventArgs
	{
		public UserMutedEventArgs (UserInfo userInfo, bool unmuted)
			: base (userInfo)
		{
			this.Unmuted = unmuted;
		}

		public bool Unmuted { get; set; }
	}

	public class UserEventArgs
		: EventArgs
	{
		public UserEventArgs (UserInfo userInfo)
		{
			if (userInfo == null)
				throw new ArgumentNullException ("userInfo");

			this.User = userInfo;
		}

		/// <summary>
		/// Gets the user target of the event.
		/// </summary>
		public UserInfo User
		{
			get;
			private set;
		}
	}

	public class ChannelChangedEventArgs
		: UserEventArgs
	{
		public ChannelChangedEventArgs (UserInfo target, ChannelInfo targetChannel, ChannelInfo previousChannel, UserInfo movedBy)
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
		public ChannelInfo TargetChannel
		{
			get; private set;
		}

		/// <summary>
		/// Gets the channel the user is being moved from.
		/// </summary>
		public ChannelInfo PreviousChannel
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets the user the target user was moved by
		/// </summary>
		public UserInfo MovedBy
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