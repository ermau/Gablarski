// Copyright (c) 2009, Eric Maupin
// All rights reserved.

// Redistribution and use in source and binary forms, with
// or without modification, are permitted provided that
// the following conditions are met:

// - Redistributions of source code must retain the above 
//   copyright notice, this list of conditions and the
//   following disclaimer.

// - Redistributions in binary form must reproduce the above
//   copyright notice, this list of conditions and the
//   following disclaimer in the documentation and/or other
//   materials provided with the distribution.

// - Neither the name of Gablarski nor the names of its
//   contributors may be used to endorse or promote products
//   derived from this software without specific prior
//   written permission.

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
using Gablarski.Messages;
using System.Threading;

namespace Gablarski.Client
{
	public class ClientUserManager
		: IClientUserManager
	{
		protected internal ClientUserManager (IClientContext context)
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

		public UserInfo this[int identifier]
		{
			get
			{
				if (users == null)
					return null;

				UserInfo user;
				lock (userLock)
				{
					this.users.TryGetValue (identifier, out user);
				}

				return user;
			}
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

			lock (userLock)
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
			lock (userLock)
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

		/// <summary>
		/// Clears the user manager of all users.
		/// </summary>
		public void Clear()
		{
			lock (userLock)
			{
				this.users = null;
			}
		}

		#region Implementation of IEnumerable

		/// <summary>
		/// Returns an enumerator that iterates through the collection.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
		/// </returns>
		/// <filterpriority>1</filterpriority>
		public IEnumerator<UserInfo> GetEnumerator()
		{
			if (this.users == null)
			{
				if (this.context.Connection == null || !this.context.Connection.IsConnected)
					throw new InvalidOperationException ("Not connected");

				this.context.Connection.Send (new RequestUserListMessage ());

				while (this.users == null)
					Thread.Sleep (1);
			}

			lock (userLock)
			{
				foreach (var user in users.Values)
					yield return user;
			}
		}

		/// <summary>
		/// Returns an enumerator that iterates through a collection.
		/// </summary>
		/// <returns>
		/// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
		/// </returns>
		/// <filterpriority>2</filterpriority>
		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}

		#endregion

		private readonly object userLock = new object();
		private Dictionary<int, UserInfo> users;
		private readonly HashSet<int> ignores = new HashSet<int> ();
		private readonly IClientContext context;

		#region Message handlers
		internal void OnUserListReceivedMessage (MessageReceivedEventArgs e)
		{
			var msg = (UserListMessage)e.Message;

			IEnumerable<UserInfo> userlist;
			lock (userLock)
			{
				this.users = msg.Users.ToDictionary (p => p.UserId, p => new UserInfo (p));
				userlist = this.users.Values.ToList();

				foreach (int ignoredId in this.ignores.ToList ().Where (id => !this.users.ContainsKey (id)))
					this.ignores.Remove (ignoredId);
			}

			OnReceivedUserList (new ReceivedListEventArgs<UserInfo> (userlist));
		}

		internal void OnMutedMessage (string username, bool unmuted)
		{
			lock (userLock)
			{
		        var user = this.users.Values.FirstOrDefault (u => u.Username == username);
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
			lock (userLock)
			{
				user = this.users[msg.UserId];
				this.users.Remove (msg.UserId);
				ignores.Remove (msg.UserId);
			}

			OnUserDisconnected (new UserEventArgs (user));
		}

		internal void OnUserLoggedInMessage (MessageReceivedEventArgs e)
		{
			var msg = (UserJoinedMessage)e.Message;

			UserInfo user;
			lock (userLock)
			{
				if (this.users == null)
					this.users = new Dictionary<int, UserInfo> ();

				user = new UserInfo (msg.UserInfo);
				this.users.Add (msg.UserInfo.UserId, user);
			}

			OnUserLoggedIn (new UserEventArgs (user));
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
			lock (userLock)
			{
				if (!this.users.TryGetValue (msg.ChangeInfo.TargetUserId, out old))
					return;

				user = this.users[msg.ChangeInfo.TargetUserId] = new UserInfo (old.Nickname, old.Username, old.UserId, msg.ChangeInfo.TargetChannelId, old.IsMuted);
				
				if (user.Equals (context.CurrentUser))
					context.CurrentUser.CurrentChannelId = msg.ChangeInfo.TargetChannelId;

				if (msg.ChangeInfo.RequestingUserId != 0)
					this.users.TryGetValue (msg.ChangeInfo.RequestingUserId, out movedBy);
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

		protected virtual void OnUserLoggedIn (UserEventArgs e)
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