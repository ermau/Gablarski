using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using Gablarski.Messages;

namespace Gablarski.Client
{
	public class ClientUserManager
		: IEnumerable<ClientUser>, INotifyCollectionChanged
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
		/// A new user has logged in.
		/// </summary>
		public event EventHandler<UserEventArgs> UserLoggedIn;

		/// <summary>
		/// A user has disconnected.
		/// </summary>
		public event EventHandler<UserEventArgs> UserDisconnected;

		/// <summary>
		/// A user was muted or ignored.
		/// </summary>
		public event EventHandler<UserEventArgs> UserMuted;

		/// <summary>
		/// A user has changed channels.
		/// </summary>
		public event EventHandler<ChannelChangedEventArgs> UserChangedChannel;

		/// <summary>
		/// Received an unsucessful result of a change channel request.
		/// </summary>
		public event EventHandler<ReceivedChannelChannelResultEventArgs> ReceivedChannelChangeResult;

		public event NotifyCollectionChangedEventHandler CollectionChanged;
		#endregion

		/// <summary>
		/// Gets the current user.
		/// </summary>
		public CurrentUser Current
		{
			get { return this.context.CurrentUser; }
		}

		public ClientUser this[int identifier]
		{
			get
			{
				if (users == null)
					return null;

				ClientUser user;
				lock (userLock)
				{
					this.users.TryGetValue (identifier, out user);
				}

				return user;
			}
		}

		/// <summary>
		/// Clears the user manager of all users.
		/// </summary>
		public void Clear()
		{
			lock (userLock)
			{
				this.users = null;
				OnCollectionChanged (new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Reset));
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
		public IEnumerator<ClientUser> GetEnumerator()
		{
			if (this.users == null)
				yield break;

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
		private Dictionary<int, ClientUser> users;
		private readonly IClientContext context;

		#region Message handlers
		internal void OnUserListReceivedMessage (MessageReceivedEventArgs e)
		{
			var msg = (UserListMessage)e.Message;

			lock (userLock)
			{
				this.users = msg.Users.ToDictionary (p => p.UserId, p => new ClientUser (p, this.context.Connection));
			}

			OnReceivedUserList (new ReceivedListEventArgs<UserInfo> (msg.Users));
			OnCollectionChanged (new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Reset));
		}

		internal void OnUserMutedMessage (string username)
		{
			lock (userLock)
			{
		        var user = this.users.Values.FirstOrDefault (u => u.Username == username);
		        if (user == null)
		            return;

		        user.Muted = true;

		        OnCollectionChanged (new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Replace, user, user));
			    OnUserMuted (new UserEventArgs (user));
			}
		}

		internal void OnUserDisconnectedMessage (MessageReceivedEventArgs e)
		{
			var msg = (UserDisconnectedMessage) e.Message;

			ClientUser user;
			lock (userLock)
			{
				user = this.users[msg.UserId];
				this.users.Remove (msg.UserId);
			}

			OnUserDisconnected (new UserEventArgs (user));
			OnCollectionChanged (new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Remove, user));
		}

		internal void OnUserLoggedInMessage (MessageReceivedEventArgs e)
		{
			var msg = (UserLoggedInMessage)e.Message;

			ClientUser user;
			lock (userLock)
			{
				if (this.users == null)
					this.users = new Dictionary<int, ClientUser>();

				user = new ClientUser (msg.UserInfo, this.context.Connection);
				this.users.Add (msg.UserInfo.UserId, user);
			}

			OnUserLoggedIn (new UserEventArgs (user));
			OnCollectionChanged (new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Add, user));
		}

		internal void OnChangeChannelResultMessage (MessageReceivedEventArgs e)
		{
		    var msg = (ChannelChangeResultMessage)e.Message;

		    if (msg.Result != ChannelChangeResult.Success)
		        return;

			//lock (this.playerLock)
			//{
			//    if (!this.players.ContainsKey (msg.MoveInfo.TargetUserId))
			//        return;

			//    this.players[msg.MoveInfo.TargetUserId].CurrentChannelId = msg.MoveInfo.TargetChannelId;
			//}

			//this.OnPlayerChangedChannnel (new ChannelChangedEventArgs (msg.MoveInfo));
		}

		internal void OnUserChangedChannelMessage (MessageReceivedEventArgs e)
		{
			var msg = (UserChangedChannelMessage)e.Message;

			ChannelInfo channel = this.context.Channels.FirstOrDefault (c => c.ChannelId.Equals (msg.ChangeInfo.TargetChannelId));
			if (channel == null)
				return;

			ClientUser old;
			ClientUser user;
			ClientUser movedBy = null;
			lock (userLock)
			{
				if (!this.users.TryGetValue (msg.ChangeInfo.TargetUserId, out old))
					return;

				this.users[msg.ChangeInfo.TargetUserId] = new ClientUser (old.Nickname, old.UserId, msg.ChangeInfo.TargetChannelId, this.context.Connection, old.Muted);
				user = this.users[msg.ChangeInfo.TargetUserId];

				if (msg.ChangeInfo.RequestingUserId != 0)
					this.users.TryGetValue (msg.ChangeInfo.RequestingUserId, out movedBy);
			}

			OnUserChangedChannnel (new ChannelChangedEventArgs (user, channel, movedBy));
			OnCollectionChanged (new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Replace, user, old));
		}
		#endregion

		#region Event Invokers
		protected virtual void OnUserMuted (UserEventArgs e)
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
			var result = this.UserLoggedIn;
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

		protected virtual void OnCollectionChanged (NotifyCollectionChangedEventArgs e)
		{
			var changed = this.CollectionChanged;
			if (changed != null)
				changed (this, e);
		}
		#endregion
	}

	#region Event Args
	public class UserEventArgs
		: EventArgs
	{
		public UserEventArgs (ClientUser userInfo)
		{
			if (userInfo == null)
				throw new ArgumentNullException ("userInfo");

			this.User = userInfo;
		}

		/// <summary>
		/// Gets the user target of the event.
		/// </summary>
		public ClientUser User
		{
			get;
			private set;
		}
	}

	public class ChannelChangedEventArgs
		: UserEventArgs
	{
		public ChannelChangedEventArgs (ClientUser target, ChannelInfo targetChannel, ClientUser movedBy)
			: base (target)
		{
			if (targetChannel == null)
				throw new ArgumentNullException ("targetChannel");

			this.TargetChannel = targetChannel;
			this.MovedBy = movedBy;
		}

		/// <summary>
		/// Gets the channel the user is being moved to.
		/// </summary>
		public ChannelInfo TargetChannel
		{
			get; private set;
		}

		/// <summary>
		/// Gets the user the target user was moved by
		/// </summary>
		public ClientUser MovedBy
		{
			get; private set;
		}
	}

	public class ReceivedChannelChannelResultEventArgs
		: EventArgs
	{
		public ReceivedChannelChannelResultEventArgs (ChannelChangeResult result)
		{
			this.Result = result;
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