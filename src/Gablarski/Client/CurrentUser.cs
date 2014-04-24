// Copyright (c) 2011-2014, Eric Maupin
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
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Gablarski.Messages;
using Cadenza;
using Tempest;

namespace Gablarski.Client
{
	public class CurrentUser
		: UserInfo, ICurrentUserHandler
	{
		public CurrentUser (IGablarskiClientContext context)
		{
			if (context == null)
				throw new ArgumentNullException ("context");

			this.context = context;

			this.context.RegisterMessageHandler<UserChangedChannelMessage> (OnUserChangedChannelMessage);
			this.context.RegisterMessageHandler<UserUpdatedMessage> (OnUserUpdatedMessage);
			this.context.RegisterMessageHandler<PermissionsMessage> (OnPermissionsMessage);
			this.context.RegisterMessageHandler<UserKickedMessage> (OnUserKickedMessage);
			this.context.RegisterMessageHandler<RegisterResultMessage> (OnRegisterResultMessage);
		}

		internal CurrentUser (IGablarskiClientContext context, int userId, string nickname, int currentChannelId)
			: this (context)
		{
			if (userId == 0)
				throw new ArgumentException ("userId");
			if (nickname.IsNullOrWhitespace())
				throw new ArgumentNullException ("nickname", "nickname is null or empty.");
			if (currentChannelId < 0)
				throw new ArgumentOutOfRangeException ("currentChannelId");

			UserId = userId;
			Nickname = nickname;
			CurrentChannelId = currentChannelId;
		}

		public event EventHandler<ReceivedRegisterResultEventArgs> ReceivedRegisterResult;

		public event EventHandler PermissionsChanged;

		public event EventHandler Kicked;

		public IEnumerable<Permission> Permissions
		{
			get
			{
				if (this.permissions == null)
					return Enumerable.Empty<Permission>();

				lock (permissionLock)
				{
					if (this.permissions == null)
						return Enumerable.Empty<Permission>();

					return this.permissions.ToList();
				}
			}
		}

		public async Task<LoginResult> LoginAsync (string username, string password)
		{
			if (username.IsNullOrWhitespace())
				throw new ArgumentNullException("username");
			if (password == null)
				throw new ArgumentNullException("password");

			try {
				var response = await this.context.Connection.SendFor<LoginResultMessage> (new LoginMessage {
					Username = username,
					Password = password
				}, 30000).ConfigureAwait (false);

				return response.Result;
			} catch (OperationCanceledException) {
				return null;
			}
		}

		public Task<LoginResultState> JoinAsync (string nickname, string serverPassword)
		{
			return JoinAsync (nickname, null, serverPassword);
		}

		public async Task<LoginResultState> JoinAsync (string nickname, string phonetic, string serverPassword)
		{
			if (nickname.IsNullOrWhitespace())
				throw new ArgumentNullException ("nickname");

			if (phonetic.IsNullOrWhitespace())
				phonetic = nickname;

			try {
				var response = await this.context.Connection.SendFor<JoinResultMessage> (new JoinMessage (nickname, phonetic, serverPassword)).ConfigureAwait (false);
				if (response.Result == LoginResultState.Success) {
					UserId = response.UserInfo.UserId;
					Username = response.UserInfo.Username;
					Nickname = response.UserInfo.Nickname;
					CurrentChannelId = response.UserInfo.CurrentChannelId;
				}

				return response.Result;
			} catch (OperationCanceledException) {
				return LoginResultState.FailedUnknown;
			}
		}

		public async Task<RegisterResult> RegisterAsync (string username, string password)
		{
			try {
				var resultMsg = await this.context.Connection.SendFor<RegisterResultMessage> (new RegisterMessage (username, password), 30000).ConfigureAwait (false);
				return resultMsg.Result;
			} catch (OperationCanceledException) {
				return RegisterResult.FailedUnknown;
			}
		}

		/// <summary>
		/// Sets the current user's comment.
		/// </summary>
		/// <param name="comment">The comment to set. <c>null</c> is valid to clear.</param>
		public async Task SetCommentAsync (string comment)
		{
			if (comment == Comment)
				return;

			Comment = comment;
			await this.context.Connection.SendAsync (new SetCommentMessage (comment)).ConfigureAwait (false);
		}

		/// <summary>
		/// Sets the current user's status.
		/// </summary>
		/// <param name="status">The status to set.</param>
		public async Task SetStatusAsync (UserStatus status)
		{
			if (status == Status)
				return;

			Status = status;
			await this.context.Connection.SendAsync (new SetStatusMessage (status)).ConfigureAwait (false);
		}

		/// <summary>
		/// Mutes all playback and sets the user's status accordingly.
		/// </summary>
		public void MutePlayback()
		{
			context.Audio.MutePlayback();

			SetStatusAsync (Status | UserStatus.MutedSound);
		}

		/// <summary>
		/// Unmutes all playback and sets the user's status accordingly.
		/// </summary>
		public void UnmutePlayback()
		{
			context.Audio.UnmutePlayback();

			SetStatusAsync (Status ^ UserStatus.MutedSound);
		}

		/// <summary>
		/// Mutes all capture and sets the user's status accordingly.
		/// </summary>
		public void MuteCapture()
		{
			context.Audio.MuteCapture();

			SetStatusAsync (Status | UserStatus.MutedMicrophone);
		}

		/// <summary>
		/// Unmutes all capture and sets the user's status accordingly.
		/// </summary>
		public void UnmuteCapture()
		{
			context.Audio.UnmuteCapture();

			SetStatusAsync (Status ^ UserStatus.MutedMicrophone);
		}

		private HashSet<int> registerResultMessagesToIgnore;
		private readonly IGablarskiClientContext context;
		private readonly object permissionLock = new object();
		private IEnumerable<Permission> permissions;

		protected virtual void OnPermissionsChanged (EventArgs e)
		{
			var changed = PermissionsChanged;
			if (changed != null)
				changed (this, e);
		}

		protected virtual void OnRegisterResult (ReceivedRegisterResultEventArgs e)
		{
			var result = ReceivedRegisterResult;
			if (result != null)
				result (this, e);
		}

		protected virtual void OnKicked (EventArgs e)
		{
			var kicked = Kicked;
			if (kicked != null)
				kicked (this, e);
		}

		internal void OnUserKickedMessage (MessageEventArgs<UserKickedMessage> e)
		{
			var msg = (UserKickedMessage)e.Message;

			if (msg.UserId != UserId)
				return;

			OnKicked (EventArgs.Empty);
		}

		internal void OnUserChangedChannelMessage (MessageEventArgs<UserChangedChannelMessage> e)
		{
			var msg = (UserChangedChannelMessage) e.Message;

			var channel = this.context.Channels[msg.ChangeInfo.TargetChannelId];
			if (channel == null)
				return;

			var user = this.context.Users[msg.ChangeInfo.TargetUserId];
			if (user == null || !user.Equals (this))
				return;

			CurrentChannelId = msg.ChangeInfo.TargetChannelId;
		}

		internal void OnUserUpdatedMessage (MessageEventArgs<UserUpdatedMessage> e)
		{
			var msg = (UserUpdatedMessage) e.Message;

			if (!msg.User.Equals (this))
				return;

			Comment = msg.User.Comment;
			Status = msg.User.Status;
		}

		internal void OnRegisterResultMessage (MessageEventArgs<RegisterResultMessage> e)
		{
			OnRegisterResult (new ReceivedRegisterResultEventArgs (e.Message.Result));
		}

		internal void OnPermissionsMessage (MessageEventArgs<PermissionsMessage> e)
		{
			if (e.Message.OwnerId != UserId)
				return;

			this.permissions = e.Message.Permissions;
			OnPermissionsChanged (EventArgs.Empty);
		}
	}
}