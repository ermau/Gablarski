using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Gablarski.Media.Sources;
using Gablarski.Messages;

namespace Gablarski.Client
{
	public partial class GablarskiClient
	{
		protected GablarskiClient()
		{
			this.Handlers = new Dictionary<ServerMessageType, Action<MessageReceivedEventArgs>>
			{
				{ ServerMessageType.ServerInfoReceived, OnServerInfoReceivedMessage },
				{ ServerMessageType.PlayerListReceived, OnPlayerListReceivedMessage },
				{ ServerMessageType.SourceListReceived, OnSourceListReceivedMessage },
				{ ServerMessageType.ChannelListReceived, OnChannelListReceivedMessage },

				{ ServerMessageType.ConnectionRejected, OnConnectionRejectedMessage },
				{ ServerMessageType.LoginResult, OnLoginResultMessage },
				{ ServerMessageType.PlayerDisconnected, OnPlayerDisconnectedMessage },
				{ ServerMessageType.ChangeChannelResult, OnChangeChannelResultMessage },

				{ ServerMessageType.SourceResult, OnSourceResultMessage },
				{ ServerMessageType.AudioDataReceived, OnAudioDataReceivedMessage },
			};
		}
		
		protected ServerInfo serverInfo;

		private string nickname;
		private long playerId;

		protected readonly Dictionary<ServerMessageType, Action<MessageReceivedEventArgs>> Handlers;
		protected readonly IClientConnection connection;

		private object sourceLock = new object ();
		private Dictionary<MediaType, IMediaSource> clientSources = new Dictionary<MediaType, IMediaSource> ();
		private Dictionary<int, IMediaSource> allSources = new Dictionary<int, IMediaSource> ();

		private object playerLock = new object();
		private Dictionary<long, PlayerInfo> players = new Dictionary<long, PlayerInfo>();

		private object channelLock = new object();
		private Dictionary<long, Channel> channels = new Dictionary<long, Channel> ();

		private void AddSource (IMediaSource source, bool mine)
		{
			lock (sourceLock)
			{
				this.allSources.Add (source.ID, source);

				if (mine)
					this.clientSources.Add (source.Type, source);
			}
		}	

		private void OnMessageReceived (object sender, MessageReceivedEventArgs e)
		{
			var msg = (e.Message as ServerMessage);
			if (msg == null)
			{
				connection.Disconnect ();
				return;
			}

			Trace.WriteLine ("[Client] Message Received: " + msg.MessageType);

			if (msg.MessageType == ServerMessageType.AudioDataReceived)
			{
				var amsg = (AudioDataReceivedMessage)msg;
				var received = this.ReceivedAudioData;
				if (received != null)
					received (this, new ReceivedAudioEventArgs (this.allSources[amsg.SourceId], amsg.Data));
			}
			else
				this.Handlers[msg.MessageType] (e);
		}

		private void OnConnectionRejectedMessage (MessageReceivedEventArgs e)
		{
			var msg = (ConnectionRejectedMessage)e.Message;

			OnConnectionRejected (new RejectedConnectionEventArgs (msg.Reason));
		}

		private void OnPlayerDisconnectedMessage (MessageReceivedEventArgs e)
		{
			var msg = (PlayerDisconnectedMessage) e.Message;

			PlayerInfo info;
			lock (playerLock)
			{
				info = this.players[msg.PlayerId];
				this.players.Remove (msg.PlayerId);
			}

			OnPlayerDisconnected (new PlayerDisconnectedEventArgs (info));
		}

		private void OnServerInfoReceivedMessage (MessageReceivedEventArgs e)
		{
			this.serverInfo = ((ServerInfoMessage)e.Message).ServerInfo;
			Trace.WriteLine ("[Client] Received server information: ");
			Trace.WriteLine ("Server name: " + this.serverInfo.ServerName);
			Trace.WriteLine ("Server description: " + this.serverInfo.ServerDescription);
		}

		#region Channels
		private void OnChangeChannelResultMessage (MessageReceivedEventArgs e)
		{
			var msg = (ChannelChangeResultMessage)e.Message;

			if (msg.Result == ChannelChangeResult.Success)
			{
				lock (playerLock)
				{
					if (!this.players.ContainsKey (msg.MoveInfo.TargetPlayerId))
						return;

					this.players[msg.MoveInfo.TargetPlayerId].CurrentChannelId = msg.MoveInfo.TargetChannelId;
				}

				OnPlayerChangedChannnel (new ChannelChangedEventArgs (msg.MoveInfo));
			}
		}

		private void OnChannelListReceivedMessage (MessageReceivedEventArgs e)
		{
			var msg = (ChannelListMessage)e.Message;

			lock (channelLock)
			{
				this.channels = msg.Channels.ToDictionary (c => c.ChannelId);
			}

			OnReceivedChannelList (new ReceivedListEventArgs<Channel> (msg.Channels));
		}
		#endregion

		#region Players
		private void OnPlayerListReceivedMessage (MessageReceivedEventArgs e)
		{
			var msg = (PlayerListMessage)e.Message;

			lock (playerLock)
			{
				this.players = msg.Players.ToDictionary (p => p.PlayerId);
			}

			OnReceivedPlayerList (new ReceivedListEventArgs<PlayerInfo> (msg.Players));
		}

		private void OnLoginResultMessage (MessageReceivedEventArgs e)
		{
			var msg = (LoginResultMessage)e.Message;

			var args = new ReceivedLoginEventArgs (msg.Result, msg.PlayerInfo);

			if (!msg.Result.Succeeded || (msg.Result.Succeeded && msg.PlayerInfo.Nickname == this.nickname))
			{
				this.playerId = msg.PlayerInfo.PlayerId;
				OnLoginResult (args);
			}
			else
			{
				lock (playerLock)
				{
					this.players.Add (msg.Result.PlayerId, msg.PlayerInfo);
				}

				OnPlayerLoggedIn (args);
			}
		}
		#endregion

		#region Sources
		private void OnSourceListReceivedMessage (MessageReceivedEventArgs e)
		{
			var msg = (SourceListMessage)e.Message;
			foreach (var sourceInfo in msg.Sources)
				this.AddSource (MediaSources.Create (Type.GetType (sourceInfo.SourceTypeName), sourceInfo.SourceId), sourceInfo.PlayerId == this.Self.PlayerId);

			OnReceivedSourceList (new ReceivedListEventArgs<MediaSourceInfo> (msg.Sources));
		}

		private void OnSourceResultMessage (MessageReceivedEventArgs e)
		{
			IMediaSource source = null;

			var sourceMessage = (SourceResultMessage)e.Message;
			if (sourceMessage.MediaSourceType == null)
			{
				Trace.WriteLine ("[Client] Source type " + sourceMessage.SourceInfo.SourceTypeName + " not found.");
			}
			else if (sourceMessage.SourceResult == SourceResult.Succeeded || sourceMessage.SourceResult == SourceResult.NewSource)
			{
				source = sourceMessage.GetSource ();
				this.AddSource (source, (sourceMessage.SourceResult == SourceResult.Succeeded));
			}

			OnReceivedSource (new ReceivedSourceEventArgs (source, sourceMessage.SourceInfo, sourceMessage.SourceResult));
		}

		private void OnAudioDataReceivedMessage (MessageReceivedEventArgs e)
		{
			var msg = (AudioDataReceivedMessage)e.Message;
			OnReceivedAudioData (new ReceivedAudioEventArgs (this.allSources[msg.SourceId], msg.Data));
		}
		#endregion
	}
}