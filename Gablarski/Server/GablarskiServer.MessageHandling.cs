using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gablarski.Audio;
using Gablarski.Messages;
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

using System.Diagnostics;
using System.Threading;

namespace Gablarski.Server
{
	public partial class GablarskiServer
	{
		protected GablarskiServer ()
		{
			this.Handlers = new Dictionary<ClientMessageType, Action<MessageReceivedEventArgs>>
			{
				{ ClientMessageType.Connect, ClientConnected },
				{ ClientMessageType.Disconnect, ClientDisconnected },
				{ ClientMessageType.Login, UserLoginAttempt },
				{ ClientMessageType.Join, UserJoinAttempt },

				{ ClientMessageType.RequestSource, ClientRequestsSource },
				{ ClientMessageType.AudioData, AudioDataReceived },
				{ ClientMessageType.ClientAudioSourceStateChange, ClientAudioSourceStateChanged },
				{ ClientMessageType.RequestMute, ClientRequestsMute },

				{ ClientMessageType.QueryServer, ClientQueryServer },
				{ ClientMessageType.RequestChannelList, ClientRequestsChannelList },
				{ ClientMessageType.RequestUserList, ClientRequestsUserList },
				{ ClientMessageType.RequestSourceList, ClientRequestsSourceList },

				{ ClientMessageType.ChangeChannel, ClientRequestsChannelChange },
				{ ClientMessageType.EditChannel, ClientEditsChannel },
			};

			this.messageRunnerThread = new Thread (this.MessageRunner);
			this.messageRunnerThread.Name = "Gablarski Server Message Runner";
		}

		private readonly Dictionary<ClientMessageType, Action<MessageReceivedEventArgs>> Handlers;

		private readonly Thread messageRunnerThread;
		private readonly Queue<MessageReceivedEventArgs> mqueue = new Queue<MessageReceivedEventArgs> (1000);

		private void MessageRunner ()
		{
			while (this.running)
			{
				if (mqueue.Count == 0)
				{
					Thread.Sleep (1);
					continue;
				}

				while (mqueue.Count > 0)
				{
					MessageReceivedEventArgs e;
					lock (mqueue)
					{
						e = mqueue.Dequeue();
					}

					var msg = (e.Message as ClientMessage);
					if (msg == null)
					{
						Disconnect (e.Connection);
						return;
					}

					Trace.WriteLineIf ((VerboseTracing || msg.MessageType != ClientMessageType.AudioData),
										"[Server] Message Received: " + msg.MessageType);

					Action<MessageReceivedEventArgs> handler;
					if (Handlers.TryGetValue (msg.MessageType, out handler))
						handler (new MessageReceivedEventArgs (e.Connection, msg));
				}
			}
		}

		protected virtual void OnMessageReceived (object sender, MessageReceivedEventArgs e)
		{
			lock (mqueue)
			{
				mqueue.Enqueue (e);
			}
		}

		protected ServerInfo GetServerInfo()
		{
			return new ServerInfo (this.settings);
		}

		private void ClientConnected (MessageReceivedEventArgs e)
		{
			var msg = (ConnectMessage)e.Message;

			if (msg.ApiVersion < MinimumApiVersion)
			{
				e.Connection.Send (new ConnectionRejectedMessage (ConnectionRejectedReason.IncompatibleVersion));
				e.Connection.Disconnect ();
				return;
			}

			e.Connection.Send (new ServerInfoMessage (GetServerInfo()));
		}

		private void ClientQueryServer (MessageReceivedEventArgs e)
		{
			if (!GetPermission (PermissionName.RequestChannelList, e.Connection))
				e.Connection.Send (new PermissionDeniedMessage (ClientMessageType.QueryServer));
			else
			{
				IEnumerable<ChannelInfo> currentChannels;
				lock (this.channelLock)
				{
					currentChannels = this.channels.Values.ToList();
				}

				e.Connection.Send (new QueryServerResultMessage { Channels = currentChannels, Users = this.connections.Users, ServerInfo = GetServerInfo() });
			}
		}

		#region Channels
		private void ClientRequestsChannelList (MessageReceivedEventArgs e)
		{
			if (!GetPermission (PermissionName.RequestChannelList, e.Connection))
				e.Connection.Send (new ChannelListMessage (GenericResult.FailedPermissions));
			else
			{
				IEnumerable<ChannelInfo> currentChannels;
				lock (this.channelLock)
				{
					currentChannels = this.channels.Values.ToList();
				}

				e.Connection.Send (new ChannelListMessage (currentChannels));
			}
		}

		private void ClientRequestsChannelChange (MessageReceivedEventArgs e)
		{
			var change = (ChannelChangeMessage)e.Message;

			ChannelChangeResult resultState = ChannelChangeResult.FailedUnknown;
			lock (this.channelLock)
			{
				if (!this.channels.ContainsKey (change.MoveInfo.TargetChannelId))
					resultState = ChannelChangeResult.FailedUnknownChannel;
			}

			UserInfo requestingPlayer;
			if (resultState == ChannelChangeResult.FailedUnknown)
			{
				requestingPlayer = this.connections[e.Connection];

				if (requestingPlayer.UserId.Equals (change.MoveInfo.TargetUserId))
				{
					if (!GetPermission (PermissionName.ChangeChannel, requestingPlayer))
						resultState = ChannelChangeResult.FailedPermissions;
				}
				else if (!GetPermission (PermissionName.ChangePlayersChannel, requestingPlayer))
					resultState = ChannelChangeResult.FailedPermissions;
				
				if (resultState == ChannelChangeResult.FailedUnknown)
				{
					if (this.connections.UpdateIfExists (e.Connection, new ServerUserInfo (requestingPlayer) { CurrentChannelId = change.MoveInfo.TargetChannelId }))
						this.connections.Send (new UserChangedChannelMessage { ChangeInfo = new ChannelChangeInfo (change.MoveInfo.TargetUserId, change.MoveInfo.TargetChannelId, requestingPlayer.UserId)});
					
					return;
				}
			}

			e.Connection.Send (new ChannelChangeResultMessage { Result = resultState });
		}

		private void ClientEditsChannel (MessageReceivedEventArgs e)
		{
			var msg = (ChannelEditMessage)e.Message;

			ChannelEditResult result = ChannelEditResult.FailedUnknown;

			ChannelInfo realChannel;
			lock (channelLock)
			{
				if (msg.Delete && this.channels.Count == 1)
					result = ChannelEditResult.FailedLastChannel;
				else if (!this.channelProvider.UpdateSupported)
					result = ChannelEditResult.FailedChannelsReadOnly;
				else if (this.channels.TryGetValue (msg.Channel.ChannelId, out realChannel) && realChannel.ReadOnly)
					result = ChannelEditResult.FailedChannelReadOnly;
				else if (msg.Channel.ChannelId != 0)
				{
					if (msg.Delete && !GetPermission (PermissionName.DeleteChannel, msg.Channel.ChannelId, e.Connection))
						result = ChannelEditResult.FailedPermissions;
					else if (!msg.Delete && !GetPermission (PermissionName.EditChannel, msg.Channel.ChannelId, e.Connection))
						result = ChannelEditResult.FailedPermissions;
				}
				else if (!GetPermission (PermissionName.AddChannel, e.Connection))
					result = ChannelEditResult.FailedPermissions;

				if (result == ChannelEditResult.FailedUnknown)
				{
					if (!msg.Delete)
						result = this.channelProvider.SaveChannel (msg.Channel);
					else
						result = this.channelProvider.DeleteChannel (msg.Channel);
				}
			}

			e.Connection.Send (new ChannelEditResultMessage (msg.Channel, result));
		}
		#endregion

		private void ClientRequestsMute (MessageReceivedEventArgs e)
		{
			var msg = (RequestMuteMessage)e.Message;

			if ((msg.Type & MuteType.User) == MuteType.User)
				ClientRequestsPlayerMute (this.connections[e.Connection], this.connections.GetUser ((int)msg.Target), msg.Unmute);
			else if ((msg.Type & MuteType.AudioSource) == MuteType.AudioSource)
			{
				AudioSource source;
				lock (this.sourceLock)
				{
					source = this.sources.FirstOrDefault (a => a.Id == (int)msg.Target);
				}

				ClientRequestsSourceMute (this.connections[e.Connection], source, msg.Unmute);
			}
		}

		#region Users
		protected void ClientRequestsPlayerMute (UserInfo requesting, UserInfo target, bool unmute)
		{
			if (!GetPermission (PermissionName.MuteUser, requesting))
				return;
			
			if (this.connections.UpdateIfExists (new ServerUserInfo (target) { IsMuted = !unmute }))
			{
				this.connections.Send (new MutedMessage
				{
					Target = target.Username,
					Type =  MuteType.User
				}, (ServerUserInfo u) => u == requesting);
			}
		}

		private void UserJoinAttempt (MessageReceivedEventArgs e)
		{
			var join = (JoinMessage)e.Message;

			if (join.Nickname.IsEmpty ())
			{
				e.Connection.Send (new JoinResultMessage (LoginResultState.FailedInvalidNickname, null));
				return;
			}

			ServerUserInfo info = this.connections[e.Connection];
			if (info == null)
			{
				if (!settings.AllowGuestLogins)
				{
					e.Connection.Send (new JoinResultMessage (LoginResultState.FailedUsername, null));
					return;
				}

				LoginResult r = AuthenticationProvider.Login (join.Nickname, null);
				if (!r.Succeeded)
				{
					e.Connection.Send (new JoinResultMessage (r.ResultState, null));
					return;
				}

				info = new ServerUserInfo (new UserInfo (join.Nickname, join.Nickname, r.UserId, defaultChannel.ChannelId, false));
				this.connections.Add (e.Connection, info);
			}
			else
			{
				info = new ServerUserInfo (info) { Nickname = join.Nickname };
				this.connections.UpdateIfExists (e.Connection, info);
			}

			LoginResultState result = LoginResultState.Success;

			if (!GetPermission (PermissionName.Login, info))
				result = LoginResultState.FailedPermissions;
			else if (this.connections.NicknameInUse (join.Nickname, e.Connection))
				result = LoginResultState.FailedNicknameInUse;
			else
				this.connections.Add (e.Connection, info);

			var msg = new JoinResultMessage (result, info);

			if (result == LoginResultState.Success)
			{
				e.Connection.Send (msg);

				if (!info.IsLoggedIn)
					e.Connection.Send (new PermissionsMessage (info.UserId, this.PermissionProvider.GetPermissions (msg.UserInfo.UserId)));

				this.connections.Send (new UserJoinedMessage (info));

				lock (this.channelLock)
				{
					e.Connection.Send (new ChannelListMessage (this.channels.Values));
				}
				
				e.Connection.Send (new UserListMessage (this.connections.Users));
				e.Connection.Send (new SourceListMessage (this.GetSourceInfoList ()));
			}
			else
			{
				e.Connection.Send (msg);
				this.connections.Remove (e.Connection);
			}
		}

		protected void UserLoginAttempt (MessageReceivedEventArgs e)
		{
			var login = (LoginMessage)e.Message;

			if (login.Username.IsEmpty())
			{
				e.Connection.Send (new LoginResultMessage (new LoginResult (0, LoginResultState.FailedUsername)));
				return;
			}

			LoginResult result = this.AuthenticationProvider.Login (login.Username, login.Password);

			e.Connection.Send (new LoginResultMessage (result));

			if (result.Succeeded)
			{
				this.connections.Add (e.Connection, new ServerUserInfo (new UserInfo (login.Username, result.UserId, this.defaultChannel.ChannelId, false), true));
				e.Connection.Send (new PermissionsMessage (result.UserId, this.PermissionProvider.GetPermissions (result.UserId)));
			}

			Trace.WriteLine ("[Server] " + login.Username + " Login: " + result.ResultState);
		}

		private void ClientRequestsUserList (MessageReceivedEventArgs e)
		{
			e.Connection.Send (new UserListMessage (this.connections.Users));
		}
		#endregion

		#region Media

		#region Sources
		protected void ClientRequestsSourceMute (UserInfo requesting, AudioSource target, bool unmute)
		{
			if (requesting == null)
				return;
			if (target == null)
				return;

			if (!GetPermission (PermissionName.MuteAudioSource, requesting))
			{
				this.connections[requesting].Send (new PermissionDeniedMessage (ClientMessageType.RequestMute));
				return;
			}

			lock (this.sourceLock)
			{
				target.Muted = !unmute;
			}

			this.connections.Send (new MutedMessage { Target = target.Id, Type = MuteType.AudioSource },
				(ServerUserInfo u) => u == requesting);
		}

		private void ClientRequestsSourceList (MessageReceivedEventArgs e)
		{
			e.Connection.Send (new SourceListMessage (this.GetSourceInfoList ()));
		}

		protected void ClientRequestsSource (MessageReceivedEventArgs e)
		{
			var request = (RequestSourceMessage)e.Message;

			SourceResult result = SourceResult.FailedUnknown;

			var user = this.connections[e.Connection];
			if (user == null || !this.GetPermission (PermissionName.RequestSource, user))
				result = SourceResult.FailedPermissions;

			AudioSource source = null;
			try
			{
				int sourceId = 1;
				lock (sourceLock)
				{
					if (this.sources.Count > 0)
						sourceId = this.sources.Max (s => s.Id) + 1;
				}

				if (result == SourceResult.FailedUnknown)
				{
					int bitrate = settings.DefaultAudioBitrate;
					if (request.TargetBitrate != 0)
						bitrate = request.TargetBitrate.Trim (settings.MinimumAudioBitrate, settings.MaximumAudioBitrate);

					source = new AudioSource (request.Name, sourceId, user.UserId, 1, bitrate, 44100, 256, 10, false);

					lock (sourceLock)
					{
						this.sources.Add (source);
						this.sourceLookup = this.sources.ToLookup (s => this.connections.GetConnection (s.OwnerId));
					}

					result = SourceResult.Succeeded;
				}
			}
			catch (OverflowException)
			{
				result = SourceResult.FailedLimit;
			}
			finally
			{
				e.Connection.Send (new SourceResultMessage (result, source));
				if (result == SourceResult.Succeeded)
				{
					connections.Send (new SourceResultMessage (SourceResult.NewSource, source), (IConnection c) => c != e.Connection);
				}
			}
		}
		#endregion

		protected void ClientAudioSourceStateChanged (MessageReceivedEventArgs e)
		{
			var msg = (ClientAudioSourceStateChangeMessage)e.Message;

			var speaker = this.connections[e.Connection];

			PermissionName n = PermissionName.SendAudioToCurrentChannel;
			if (speaker.CurrentChannelId != msg.ChannelId)
				n = PermissionName.SendAudioToDifferentChannel;

			if (!GetPermission (n, msg.ChannelId, speaker.UserId))
				return;

			this.connections.Send (new AudioSourceStateChangeMessage (msg.Starting, msg.SourceId, msg.ChannelId), (con, user) => con != e.Connection && user.CurrentChannelId.Equals (msg.ChannelId));
		}

		protected void AudioDataReceived (MessageReceivedEventArgs e)
		{
			var msg = (SendAudioDataMessage)e.Message;
			var speaker = this.connections[e.Connection];

			PermissionName n = PermissionName.SendAudioToCurrentChannel;
			if (speaker.CurrentChannelId == msg.TargetChannelId)
				n = PermissionName.SendAudioToDifferentChannel;

			if (!GetPermission (n, msg.TargetChannelId, speaker.UserId))
				return;

			this.connections.Send (new AudioDataReceivedMessage (msg.SourceId, msg.Sequence, msg.Data), (con, user) => con != e.Connection && !user.IsMuted && user.CurrentChannelId.Equals (msg.TargetChannelId));
		}
		#endregion
	}
}