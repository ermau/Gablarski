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

		public event EventHandler<ReceivedJoinResultEventArgs> ReceivedJoinResult;

		public event EventHandler PermissionsChanged;
		#endregion

		/// <summary>
		/// Logs into the connected server
		/// </summary>
		/// <param name="username">The username to log in with.</param>
		/// <param name="password">The password to log in with.</param>
		/// <exception cref="System.ArgumentNullException"><paramref name="username"/> is null or empty.</exception>
		/// <exception cref="System.ArgumentNullException"><paramref name="password"/> is null.</exception>
		public void Login (string username, string password)
		{
			if (username.IsEmpty())
				throw new ArgumentNullException("username");
			if (password == null)
				throw new ArgumentNullException("password");

			this.context.Connection.Send (new LoginMessage
			{
				Username = username,
				Password = password
			});
		}

		/// <summary>
		/// Join
		/// </summary>
		/// <param name="nickname"></param>
		public void Join (string nickname)
		{
			if (nickname.IsEmpty())
				throw new ArgumentNullException ("nickname");

			this.context.Connection.Send (new JoinMessage (nickname));
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

		protected virtual void OnJoinResult (ReceivedJoinResultEventArgs e)
		{
			var join = this.ReceivedJoinResult;
			if (join != null)
				join (this, e);
		}

		internal void OnLoginResultMessage (MessageReceivedEventArgs e)
		{
		    var msg = (LoginResultMessage)e.Message;
			if (msg.Result.ResultState == LoginResultState.Success)
				this.UserId = msg.Result.UserId;

			var args = new ReceivedLoginResultEventArgs (msg.Result);
			OnLoginResult (args);
		}

		internal void OnJoinResultMessage (MessageReceivedEventArgs e)
		{
			var msg = (JoinResultMessage)e.Message;
			if (msg.Result == LoginResultState.Success)
			{
				this.Username = msg.UserInfo.Username;
				this.Nickname = msg.UserInfo.Nickname;
				this.CurrentChannelId = msg.UserInfo.CurrentChannelId;
			}

			var args = new ReceivedJoinResultEventArgs(msg.Result);
			OnJoinResult (args);
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

	public class ReceivedJoinResultEventArgs
		: EventArgs
	{
		public ReceivedJoinResultEventArgs (LoginResultState result)
		{
			this.Result = result;
		}

		public LoginResultState Result
		{
			get; set;
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