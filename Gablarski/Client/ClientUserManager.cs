using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gablarski.Messages;

namespace Gablarski.Client
{
	public class ClientUserManager
		: IEnumerable<ClientUser>
	{
		protected internal ClientUserManager (IClientContext context)
		{
			if (context == null)
				throw new ArgumentNullException ("context");

			this.context = context;
		}

		#region Events
		/// <summary>
		/// An new or updated player list has been received.
		/// </summary>
		public event EventHandler<ReceivedListEventArgs<UserInfo>> ReceivedUserList;

		/// <summary>
		/// A new player has logged in.
		/// </summary>
		public event EventHandler<UserLoggedInEventArgs> UserLoggedIn;

		/// <summary>
		/// A player has disconnected.
		/// </summary>
		public event EventHandler<UserDisconnectedEventArgs> UserDisconnected;

		/// <summary>
		/// A player has changed channels.
		/// </summary>
		public event EventHandler<ChannelChangedEventArgs> UserChangedChannel;

		/// <summary>
		/// Received an unsucessful result of a change channel request.
		/// </summary>
		public event EventHandler<ReceivedChannelChannelResultEventArgs> ReceivedChannelChangeResult;
		#endregion

		public CurrentUser Current
		{
			get { return this.context.CurrentUser; }
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
		private Dictionary<object, ClientUser> users;
		private readonly IClientContext context;

		internal ClientUser this[object identifier]
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

		#region Message handlers
		internal void OnUserListReceivedMessage (MessageReceivedEventArgs e)
		{
			var msg = (UserListMessage)e.Message;

			lock (userLock)
			{
				this.users = msg.Users.ToDictionary (p => p.UserId, p => new ClientUser (p, this.context.Connection));
			}

			this.OnReceivedUserList (new ReceivedListEventArgs<UserInfo> (msg.Users));
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

			this.OnUserDisconnected (new UserDisconnectedEventArgs (user));
		}

		internal void OnUserLoggedInMessage (MessageReceivedEventArgs e)
		{
			var msg = (UserLoggedInMessage)e.Message;

			ClientUser user;
			lock (userLock)
			{
				if (this.users == null)
					this.users = new Dictionary<object, ClientUser>();

				user = new ClientUser (msg.UserInfo, this.context.Connection);
				this.users.Add (msg.UserInfo.UserId,user);
			}

			this.OnUserLoggedIn (new UserLoggedInEventArgs (user));
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

			Channel channel = this.context.Channels.FirstOrDefault (c => c.ChannelId.Equals (msg.ChangeInfo.TargetChannelId));
			if (channel == null)
				return;

			ClientUser user;
			ClientUser movedBy = null;
			lock (userLock)
			{
				if (!this.users.ContainsKey (msg.ChangeInfo.TargetUserId))
					return;

				var old = this.users[msg.ChangeInfo.TargetUserId];
				this.users[msg.ChangeInfo.TargetUserId] = new ClientUser (old.Nickname, old.UserId, msg.ChangeInfo.TargetChannelId, this.context.Connection);
				user = this.users[msg.ChangeInfo.TargetUserId];

				if (msg.ChangeInfo.RequestingUserId != null)
					this.users.TryGetValue (msg.ChangeInfo.RequestingUserId, out movedBy);
			}

			this.OnUserChangedChannnel (new ChannelChangedEventArgs (user, channel, movedBy));
		}
		#endregion

		#region Event Invokers
		protected virtual void OnUserDisconnected (UserDisconnectedEventArgs e)
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

		protected virtual void OnUserLoggedIn (UserLoggedInEventArgs e)
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
		#endregion
	}

	#region Event Args
	public class UserLoggedInEventArgs
		: EventArgs
	{
		public UserLoggedInEventArgs (ClientUser userInfo)
		{
			if (userInfo == null)
				throw new ArgumentNullException ("userInfo");

			this.UserInfo = userInfo;
		}

		/// <summary>
		/// Gets the new user's information.
		/// </summary>
		public ClientUser UserInfo
		{
			get;
			private set;
		}
	}

	public class UserDisconnectedEventArgs
		: EventArgs
	{
		public UserDisconnectedEventArgs (ClientUser user)
		{
			if (user == null)
				throw new ArgumentNullException ("user");

			this.User = user;
		}

		/// <summary>
		/// Gets the user that disconnected.
		/// </summary>
		public ClientUser User
		{
			get;
			private set;
		}
	}

	public class ChannelChangedEventArgs
		: EventArgs
	{
		public ChannelChangedEventArgs (ClientUser target, Channel targetChannel, ClientUser movedBy)
		{
			if (target == null)
				throw new ArgumentNullException ("target");
			if (targetChannel == null)
				throw new ArgumentNullException ("targetChannel");

			this.TargetUser = target;
			this.TargetChannel = targetChannel;
			this.MovedBy = movedBy;
		}

		/// <summary>
		/// Gets the target user of the move.
		/// </summary>
		public ClientUser TargetUser
		{
			get; private set;
		}

		/// <summary>
		/// Gets the channel the user is being moved to.
		/// </summary>
		public Channel TargetChannel
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