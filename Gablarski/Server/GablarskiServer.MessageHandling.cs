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
				{ ClientMessageType.RequestPlayerList, ClientRequestsPlayerList },
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
					Thread.Sleep (0);
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

				#if !DEBUG
				if (this.Handlers.ContainsKey (msg.MessageType))
				#endif
					this.Handlers[msg.MessageType] (e);
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
		}

		#region Channels
		private void ClientRequestsChannelList (MessageReceivedEventArgs e)
		{
			if (!GetPermission (PermissionName.RequestChannelList, e.Connection))
			{
				lock (this.channelLock)
				{
					e.Connection.Send (new ChannelListMessage (this.channels.Values));
				}
			}
		}

		private void ClientRequestsChannelChange (MessageReceivedEventArgs e)
		{
			var change = (ChangeChannelMessage)e.Message;

			ChannelChangeResult resultState = ChannelChangeResult.FailedUnknown;
			lock (this.channelLock)
			{
				if (!this.channels.ContainsKey (change.MoveInfo.TargetChannelId))
					resultState = ChannelChangeResult.FailedUnknownChannel;
			}

			PlayerInfo requestingPlayer = null;
			if (resultState == ChannelChangeResult.FailedUnknown)
			{
				requestingPlayer = this.connections[e.Connection];

				if (requestingPlayer.PlayerId == change.MoveInfo.TargetPlayerId)
				{
					if (!GetPermission (PermissionName.ChangeChannel, requestingPlayer))
						resultState = ChannelChangeResult.FailedPermissions;
				}
				else if (!GetPermission (PermissionName.ChangePlayersChannel, requestingPlayer))
					resultState = ChannelChangeResult.FailedPermissions;
				
				if (resultState == ChannelChangeResult.FailedUnknown)
				{
					requestingPlayer.CurrentChannelId = change.MoveInfo.TargetChannelId;
					this.connections.Send (new ChannelChangeResultMessage (requestingPlayer.PlayerId, change.MoveInfo));
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
				else if (msg.Channel.ChannelId != 0)
				{
					if (msg.Delete && !GetPermission (PermissionName.DeleteChannel, msg.Channel.ChannelId, e.Connection))
						result = ChannelEditResult.FailedPermissions;
					else if (!msg.Delete && !GetPermission (PermissionName.EditChannel, msg.Channel.ChannelId, e.Connection))
						result = ChannelEditResult.FailedPermissions;
				}
				else if (msg.Channel.ChannelId == 0 && !GetPermission (PermissionName.AddChannel, e.Connection))
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

		#region Players
		protected void UserLoginAttempt (MessageReceivedEventArgs e)
		{
			var login = (LoginMessage)e.Message;

			LoginResult result = this.UserProvider.Login (login.Username, login.Password);
			PlayerInfo info = null;

			if (result.Succeeded)
			{
				info = new PlayerInfo (login.Nickname, result.PlayerId, this.defaultChannel.ChannelId);

				if (!this.GetPermission (PermissionName.Login, info))
					result.ResultState = LoginResultState.FailedPermissions;
				else if (!this.connections.PlayerLoggedIn (login.Nickname))
					this.connections.Add (e.Connection, info);
				else
					result.ResultState = LoginResultState.FailedNicknameInUse;
			}

			var msg = new LoginResultMessage (result, info);

			if (result.Succeeded)
			{
				this.connections.Send (msg);

				e.Connection.Send (new ServerInfoMessage (new ServerInfo (this.settings)));

				lock (this.channelLock)
				{
					e.Connection.Send (new ChannelListMessage (this.channels.Values));
				}
				
				e.Connection.Send (new PlayerListMessage (this.connections.Players));
				e.Connection.Send (new SourceListMessage (this.GetSourceInfoList ()));
			}
			else
				e.Connection.Send (msg);

			Trace.WriteLine ("[Server]" + login.Username + " Login: " + result.ResultState);
		}

		private void ClientRequestsPlayerList (MessageReceivedEventArgs e)
		{
			e.Connection.Send (new PlayerListMessage (this.connections.Players));
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
			int sourceId = -1;

			var player = this.connections[e.Connection];
			if (player == null || !this.GetPermission (PermissionName.RequestSource, player))
				result = SourceResult.FailedPermissions;

			IMediaSource source = null;
			try
			{
				int index = 0;
				lock (sourceLock)
				{
					if (!sources.ContainsKey (e.Connection))
						sources.Add (e.Connection, new List<IMediaSource> ());

					if (!sources[e.Connection].Any (s => s != null && s.GetType () == request.MediaSourceType))
					{
						sourceId = sources.Sum (kvp => kvp.Value.Count);
						index = sources[e.Connection].Count;
						sources[e.Connection].Add (null);
					}
					else
						result = SourceResult.FailedPermittedSingleSourceOfType;
				}

				if (result == SourceResult.FailedUnknown)
				{
					source = MediaSources.Create (request.MediaSourceType, sourceId);
					if (source != null)
					{
						lock (sourceLock)
						{
							sources[e.Connection][index] = source;
						}

						result = SourceResult.Succeeded;
					}
					else
						result = SourceResult.FailedNotSupportedType;
				}
			}
			catch (OverflowException)
			{
				result = SourceResult.FailedLimit;
			}
			finally
			{
				MediaSourceInfo sourceInfo = new MediaSourceInfo
				{
					SourceId = sourceId,
					PlayerId = player.PlayerId,
					MediaType = (source != null) ? source.Type : MediaType.None,
					SourceTypeName = request.MediaSourceType.AssemblyQualifiedName
				};

				e.Connection.Send (new SourceResultMessage (result, sourceInfo));
				if (result == SourceResult.Succeeded)
				{
					connections.Send (new SourceResultMessage (SourceResult.NewSource, sourceInfo), (IConnection c) => c != e.Connection);
				}
			}
		}
		#endregion

		protected void AudioDataReceived (MessageReceivedEventArgs e)
		{
			var msg = (SendAudioDataMessage)e.Message;

			this.connections.Send (new AudioDataReceivedMessage (msg.SourceId, msg.Data), (c, p) => c != e.Connection && p.CurrentChannelId == msg.TargetChannelId);
		}
		#endregion
	}
}