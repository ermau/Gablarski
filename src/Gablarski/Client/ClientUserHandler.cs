// Copyright (c) 2009, Eric Maupin
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
		protected internal ClientUserHandler (IClientContext context)
		{
			if (context == null)
				throw new ArgumentNullException ("context");

			this.context = context;
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

		/// <summary>
		/// Gets whether or not <paramref name="user"/> is currently ignored.
		/// </summary>
		/// <param name="user">The user to check.</param>
		/// <returns><c>true</c> if ignored, <c>false</c> if not.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="user"/> is <c>null</c>.</exception>
		public bool GetIsIgnored (UserInfo user)
		{
			if (user == null)
				throw new ArgumentNullException ("user");

			lock (SyncRoot)
			{
				return ignores.Contains (user.UserId);
			}
		}

		/// <summary>
		/// Toggles <paramref name="user"/>'s ignored status.
		/// </summary>
		/// <param name="user">The user to toggle ignored status.</param>
		/// <returns><c>true</c> if the user is now ignored, <c>false</c> if now unignored.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="user"/> is <c>null</c>.</exception>
		public bool ToggleIgnore (UserInfo user)
		{
			if (user == null)
				throw new ArgumentNullException ("user");

			bool ignored;
			lock (SyncRoot)
			{
				ignored = ignores.Contains (user.UserId);

				if (ignored)
					ignores.Remove (user.UserId);
				else
					ignores.Add (user.UserId);
			}

			return !ignored;
		}

		/// <summary>
		/// Requests a mute toggle for <paramref name="user"/>.
		/// </summary>
		/// <param name="user">The user to attempt to mute or unmute.</param>
		/// <exception cref="ArgumentNullException"><paramref name="user"/> is <c>null</c>.</exception>
		public void ToggleMute (UserInfo user)
		{
			if (user == null)
				throw new ArgumentNullException ("user");

			context.Connection.Send (new RequestMuteMessage { Target = user.Username, Type = MuteType.User, Unmute = user.IsMuted });
		}

		public IEnumerable<UserInfo> GetUsersInChannel (int channelId)
		{
			lock (SyncRoot)
			{
				return this.channels[channelId].ToList();
			}
		}

		public bool TryGetUser (int userId, out UserInfo user)
		{
			lock (SyncRoot)
			{
				return users.TryGetUser (userId, out user);
			}
		}

		public UserInfo this[int key]
		{
			get
			{
				UserInfo user;
				lock (SyncRoot)
				{
					this.users.TryGetUser (key, out user);
				}

				return user;
			}
		}

		public void Reset()
		{
			lock (SyncRoot)
			{
				users.Clear();
				ignores.Clear();
				channels.Clear();
			}
		}

		public IEnumerator<UserInfo> GetEnumerator ()
		{
			IEnumerable<UserInfo> returnUsers;
			lock (SyncRoot)
			{
				 returnUsers = this.users.ToList();
			}

			return returnUsers.GetEnumerator ();
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator ()
		{
			return GetEnumerator ();
		}

		protected readonly object SyncRoot = new object ();

		private readonly IClientUserManager users = new ClientUserManager();
		private readonly MutableLookup<int, UserInfo> channels = new MutableLookup<int, UserInfo> ();

		private readonly HashSet<int> ignores = new HashSet<int> ();
		private readonly IClientContext context;

		#region Message handlers
		internal void OnUserListReceivedMessage (MessageReceivedEventArgs e)
		{
			var msg = (UserListMessage)e.Message;

			IEnumerable<UserInfo> userlist;
			lock (SyncRoot)
			{
				users.Update (msg.Users);
				userlist = msg.Users.ToList();

				foreach (int ignoredId in this.ignores.Where (id => !GetIsIgnored (this[id])).ToList ())
					this.ignores.Remove (ignoredId);
			}

			OnReceivedUserList (new ReceivedListEventArgs<UserInfo> (userlist));
		}

		internal void OnMutedMessage (string username, bool unmuted)
		{
			lock (SyncRoot)
			{
		        var user = this.SingleOrDefault (u => u.Username == username);
		        if (user == null)
		            return;

		        user.IsMuted = true;

			    OnUserMuted (new UserMutedEventArgs (user, unmuted));
			}
		}

		internal void OnUserDisconnectedMessage (MessageReceivedEventArgs e)
		{
			var msg = (UserDisconnectedMessage) e.Message;

			UserInfo user;
			lock (SyncRoot)
			{
				if (!this.TryGetUser (msg.UserId, out user))
					return;

				users.Depart (user);
				ignores.Remove (msg.UserId);
			}

			OnUserDisconnected (new UserEventArgs (user));
		}

		internal void OnUserJoinedMessage (MessageReceivedEventArgs e)
		{
			var msg = (UserJoinedMessage)e.Message;

			UserInfo user = new UserInfo (msg.UserInfo);
			
			users.Join (user);

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
				if (!TryGetUser (msg.ChangeInfo.TargetUserId, out old))
					return;

				user = new UserInfo (old.Nickname, old.Phonetic, old.Username, old.UserId, msg.ChangeInfo.TargetChannelId, old.IsMuted);
				users.Update (user);
				
				if (user.Equals (context.CurrentUser))
					context.CurrentUser.CurrentChannelId = msg.ChangeInfo.TargetChannelId;

				if (msg.ChangeInfo.RequestingUserId != 0)
					TryGetUser (msg.ChangeInfo.RequestingUserId, out movedBy);
			}

			OnUserChangedChannnel (new ChannelChangedEventArgs (user, channel, previous, movedBy));
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