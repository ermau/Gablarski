using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gablarski.Messages;
using Gablarski.Server;

namespace Gablarski.Client
{
	public class CurrentUser
		: ClientUser
	{
		internal CurrentUser (IClientContext context)
			: base (context.Connection)
		{
			if (context == null)
				throw new ArgumentNullException ("context");

			this.context = context;
		}

		internal CurrentUser (IClientContext context, int userId, string nickname, int currentChannelId)
			: this (context)
		{
			if (userId < 0)
				throw new ArgumentOutOfRangeException("userId");
			if (nickname.IsEmpty())
				throw new ArgumentNullException("nickname", "nickname is null or empty.");
			if (currentChannelId < 0)
				throw new ArgumentOutOfRangeException("currentChannelId");

			this.UserId = userId;
			this.Nickname = nickname;
			this.CurrentChannelId = currentChannelId;
		}

		public IEnumerable<Permission> Permissions
		{
			get
			{
				lock (permissionLock)
				{
					return this.permissions.ToList();
				}
			}
		}

		#region Events
		/// <summary>
		/// A login result has been received.
		/// </summary>
		public event EventHandler<ReceivedLoginResultEventArgs> ReceivedLoginResult;

		public event EventHandler PermissionsChanged;
		#endregion

		/// <summary>
		/// Logs into the connected server with <paramref name="nick"/>.
		/// </summary>
		/// <param name="nick">The nickname to log in with.</param>
		/// <exception cref="System.ArgumentNullException"><paramref name="nick"/> is null or empty.</exception>
		public void Login (string nick)
		{
			Login (nick, null, null);
		}

		/// <summary>
		/// Logs into the connected server with <paramref name="nick"/>.
		/// </summary>
		/// <param name="nick">The nickname to log in with.</param>
		/// <param name="username">The username to log in with.</param>
		/// <param name="password">The password to log in with.</param>
		/// <exception cref="System.ArgumentNullException"><paramref name="nick"/> is null or empty.</exception>
		public void Login (string nick, string username, string password)
		{
			if (nick.IsEmpty())
				throw new ArgumentNullException ("nick", "nick must not be null or empty");

			this.context.Connection.Send (new LoginMessage
			{
				Nickname = nick,
				Username = username,
				Password = password
			});
		}

		private readonly IClientContext context;
		private readonly object permissionLock = new object();
		private IEnumerable<Permission> permissions;

		protected virtual void OnLoginResult (ReceivedLoginResultEventArgs e)
		{
			var result = this.ReceivedLoginResult;
			if (result != null)
				result (this, e);
		}

		protected virtual void OnPermissionsChanged (EventArgs e)
		{
			var changed = this.PermissionsChanged;
			if (changed != null)
				changed (this, e);
		}

		internal void OnLoginResultMessage (MessageReceivedEventArgs e)
		{
		    var msg = (LoginResultMessage)e.Message;
			if (msg.Result.ResultState == LoginResultState.Success)
			{
				this.UserId = msg.UserInfo.UserId;
				this.Nickname = msg.UserInfo.Nickname;
				this.CurrentChannelId = msg.UserInfo.CurrentChannelId;
			}

			var args = new ReceivedLoginResultEventArgs (msg.Result);
			this.OnLoginResult (args);
		}

		internal void OnPermissionsMessage (MessageReceivedEventArgs e)
		{
			var msg = (PermissionsMessage)e.Message;
			if (msg.OwnerId != this.UserId)
				return;

			this.permissions = msg.Permissions;
			OnPermissionsChanged (EventArgs.Empty);
		}
	}

	public class ReceivedLoginResultEventArgs
		: EventArgs
	{
		public ReceivedLoginResultEventArgs (LoginResult result)
		{
			this.Result = result;
		}

		public LoginResult Result
		{
			get;
			private set;
		}
	}
}