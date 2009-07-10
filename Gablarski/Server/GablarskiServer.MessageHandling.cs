using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gablarski.Messages;
using Gablarski.Media.Sources;
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
				{ ClientMessageType.RequestSource, ClientRequestsSource },
				{ ClientMessageType.AudioData, AudioDataReceived },

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

				MessageReceivedEventArgs e;
				lock (mqueue)
				{
					e = mqueue.Dequeue ();
				}

				var msg = (e.Message as ClientMessage);
				if (msg == null)
				{
					Disconnect (e.Connection);
					return;
				}

				Trace.WriteLineIf ((msg.MessageType != ClientMessageType.AudioData), "[Server] Message Received: " + msg.MessageType);

				Action<MessageReceivedEventArgs> handler;
				Handlers.TryGetValue (msg.MessageType, out handler);
				if (handler != null)
					handler (new MessageReceivedEventArgs (e.Connection, msg));
			}
		}

		protected virtual void OnMessageReceived (object sender, MessageReceivedEventArgs e)
		{
			lock (mqueue)
			{
				mqueue.Enqueue (e);
			}
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

			e.Connection.Send (
				new ServerInfoMessage (new ServerInfo (this.settings, this.ChannelProvider.IdentifyingType,
				                                       this.UserProvider.IdentifyingType)));
		}

		#region Channels
		private void ClientRequestsChannelList (MessageReceivedEventArgs e)
		{
			if (!GetPermission (PermissionName.RequestChannelList, e.Connection))
				e.Connection.Send (new ChannelListMessage (GenericResult.FailedPermissions));
			else
			{
				lock (this.channelLock)
				{
					e.Connection.Send (new ChannelListMessage (this.channels.Values));
				}
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

				if (requestingPlayer.UserId == change.MoveInfo.TargetUserId)
				{
					if (!GetPermission (PermissionName.ChangeChannel, requestingPlayer))
						resultState = ChannelChangeResult.FailedPermissions;
				}
				else if (!GetPermission (PermissionName.ChangePlayersChannel, requestingPlayer))
					resultState = ChannelChangeResult.FailedPermissions;
				
				if (resultState == ChannelChangeResult.FailedUnknown)
				{
					requestingPlayer.CurrentChannelId = change.MoveInfo.TargetChannelId;
					this.connections.Send (new ChannelChangeMessage (change.MoveInfo.TargetUserId, change.MoveInfo.TargetChannelId, requestingPlayer));
					return;
				}
			}

			e.Connection.Send (new ChannelChangeResultMessage { Result = resultState });
		}

		private void ClientEditsChannel (MessageReceivedEventArgs e)
		{
			var msg = (ChannelEditMessage)e.Message;

			ChannelEditResult result = ChannelEditResult.FailedUnknown;

			Channel realChannel;
			lock (channelLock)
			{
				if (!this.channels.TryGetValue (msg.Channel.ChannelId, out realChannel))
					result = ChannelEditResult.FailedChannelDoesntExist;
				else if (!this.channelProvider.UpdateSupported)
					result = ChannelEditResult.FailedChannelsReadOnly;
				else if (realChannel.ReadOnly)
					result = ChannelEditResult.FailedChannelReadOnly;
				else if (msg.Channel.ChannelId != null)
				{
					if (msg.Delete && !GetPermission (PermissionName.DeleteChannel, msg.Channel.ChannelId, e.Connection))
						result = ChannelEditResult.FailedPermissions;
					else if (!msg.Delete && !GetPermission (PermissionName.EditChannel, msg.Channel.ChannelId, e.Connection))
						result = ChannelEditResult.FailedPermissions;
				}
				else if (msg.Channel.ChannelId == null && !GetPermission (PermissionName.AddChannel, e.Connection))
					result = ChannelEditResult.FailedPermissions;

				if (result == ChannelEditResult.FailedUnknown)
				{
					if (!msg.Delete)
						this.channelProvider.SaveChannel (msg.Channel);
					else
						this.channelProvider.DeleteChannel (msg.Channel);

					this.UpdateChannels (true);
					result = ChannelEditResult.Success;
				}
			}

			e.Connection.Send (new ChannelEditResultMessage (msg.Channel, result));
		}
		#endregion

		#region Users
		protected void UserLoginAttempt (MessageReceivedEventArgs e)
		{
			var login = (LoginMessage)e.Message;

			if (login.Nickname.IsEmpty ())
			{
				e.Connection.Send (new LoginResultMessage (new LoginResult (0, LoginResultState.FailedInvalidNickname), null));
				return;
			}

			LoginResult result = this.UserProvider.Login (login.Username, login.Password);
			UserInfo info = null;

			if (result.Succeeded)
			{
				info = new UserInfo (login.Nickname, result.UserId, this.defaultChannel.ChannelId);

				if (!this.GetPermission (PermissionName.Login, info))
					result.ResultState = LoginResultState.FailedPermissions;
				else if (!this.connections.UserLoggedIn (login.Nickname))
					this.connections.Add (e.Connection, info);
				else
					result.ResultState = LoginResultState.FailedNicknameInUse;
			}

			var msg = new LoginResultMessage (result, info);

			if (result.Succeeded)
			{
				e.Connection.Send (msg);

				this.connections.Send (new UserLoggedInMessage (info));

				lock (this.channelLock)
				{
					e.Connection.Send (new ChannelListMessage (this.channels.Values));
				}
				
				e.Connection.Send (new UserListMessage (this.connections.Users));
				e.Connection.Send (new SourceListMessage (this.GetSourceInfoList ()));
			}
			else
				e.Connection.Send (msg);

			Trace.WriteLine ("[Server]" + login.Username + " Login: " + result.ResultState);
		}

		private void ClientRequestsUserList (MessageReceivedEventArgs e)
		{
			e.Connection.Send (new UserListMessage (this.connections.Users));
		}
		#endregion

		#region Media

		#region Sources
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
				int index;
				int sourceId;
				lock (sourceLock)
				{
					if (!sources.ContainsKey (e.Connection))
						sources.Add (e.Connection, new List<AudioSource> ());

					//if (!sources[e.Connection].Any (s => s != null && s.GetType () == request.MediaSourceType))
					//{
					sourceId = sources.Sum (kvp => kvp.Value.Count) + 1;
					index = sources[e.Connection].Count;
					sources[e.Connection].Add (null);
					//}-
					//else
					//    result = SourceResult.FailedPermittedSingleSourceOfType;
				}

				if (result == SourceResult.FailedUnknown)
				{
					int bitrate = settings.DefaultAudioBitrate;
					if (request.TargetBitrate != 0)
						bitrate = request.TargetBitrate.Trim (settings.MinimumAudioBitrate, settings.MaximumAudioBitrate);

					//source = MediaSources.Create (request.MediaSourceType, sourceId, user.UserId);
					source = new AudioSource (request.Name, sourceId, user.UserId, 1, bitrate, 44100, 512);
					//if (source != null)
					//{
					lock (sourceLock)
					{
						sources[e.Connection][index] = source;
					}

					result = SourceResult.Succeeded;
					//}
					//else
					//    result = SourceResult.FailedNotSupportedType;
				}
			}
			catch (OverflowException)
			{
				result = SourceResult.FailedLimit;
			}
			finally
			{
				//MediaSourceInfo sourceInfo = new MediaSourceInfo
				//{
				//    SourceId = sourceId,
				//    UserId = user.UserId,
				//    MediaType = (source != null) ? source.Type : MediaType.None,
				//    SourceTypeName = request.MediaSourceType.AssemblyQualifiedName
				//};

				e.Connection.Send (new SourceResultMessage (result, source));
				if (result == SourceResult.Succeeded)
				{
					connections.Send (new SourceResultMessage (SourceResult.NewSource, source), (IConnection c) => c != e.Connection);
				}
			}
		}
		#endregion

		protected void AudioDataReceived (MessageReceivedEventArgs e)
		{
			var msg = (SendAudioDataMessage)e.Message;

			this.connections.Send (new AudioDataReceivedMessage (msg.SourceId, msg.Data), (c, p) => c != e.Connection && p.CurrentChannelId.Equals (msg.TargetChannelId));
		}
		#endregion
	}
}