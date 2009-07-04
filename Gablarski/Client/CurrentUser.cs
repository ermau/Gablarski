using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gablarski.Messages;

namespace Gablarski.Client
{
	public class CurrentUser
		: ClientUser
	{
		internal CurrentUser (IClientContext context)
		{
			if (context == null)
				throw new ArgumentNullException ("context");

			this.context = context;
		}

		internal CurrentUser (IClientContext context, object userId, string nickname, object currentChannelId)
			: this (context)
		{
			if (userId == null)
				throw new ArgumentNullException("userId");
			if (nickname.IsEmpty())
				throw new ArgumentNullException("nickname", "nickname is null or empty.");
			if (currentChannelId == null)
				throw new ArgumentNullException("currentChannelId");

			this.UserId = userId;
			this.Nickname = nickname;
			this.CurrentChannelId = currentChannelId;
		}

		#region Events
		/// <summary>
		/// A login result has been received.
		/// </summary>
		public event EventHandler<ReceivedLoginResultEventArgs> ReceivedLoginResult;
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

		protected virtual void OnLoginResult (ReceivedLoginResultEventArgs e)
		{
			var result = this.ReceivedLoginResult;
			if (result != null)
				result (this, e);
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