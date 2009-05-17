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
				{ ServerMessageType.LoginResult, OnLoginResultMessage },
				{ ServerMessageType.SourceResult, OnSourceResultMessage },
				{ ServerMessageType.AudioDataReceived, OnAudioDataReceivedMessage },
				{ ServerMessageType.PlayerDisconnected, OnPlayerDisconnectedMessage },
			};
		}
		
		protected ServerInfo serverInfo;

		private string nickname;
		private long userId;

		protected readonly Dictionary<ServerMessageType, Action<MessageReceivedEventArgs>> Handlers;
		protected readonly IClientConnection connection;

		private object sourceLock = new object ();
		private readonly Dictionary<MediaType, IMediaSource> clientSources = new Dictionary<MediaType, IMediaSource> ();
		private readonly Dictionary<int, IMediaSource> allSources = new Dictionary<int, IMediaSource> ();

		private object playerLock = new object();
		private readonly Dictionary<long, PlayerInfo> players = new Dictionary<long, PlayerInfo>();

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

		private void OnPlayerListReceivedMessage (MessageReceivedEventArgs e)
		{
			var msg = (PlayerListMessage)e.Message;

			lock (playerLock)
			{
				foreach (PlayerInfo player in msg.Players)
					this.players.Add (player.PlayerId, player);
			}

			OnReceivedPlayerList (new ReceivedListEventArgs<PlayerInfo> (msg.Players));
		}

		private void OnSourceListReceivedMessage (MessageReceivedEventArgs e)
		{
			var msg = (SourceListMessage)e.Message;
			foreach (var sourceInfo in msg.Sources)
				this.AddSource (MediaSources.Create (Type.GetType (sourceInfo.SourceTypeName), sourceInfo.SourceId), sourceInfo.PlayerId == this.userId);

			OnReceivedSourceList (new ReceivedListEventArgs<MediaSourceInfo> (msg.Sources));
		}

		private void OnLoginResultMessage (MessageReceivedEventArgs e)
		{
			var msg = (LoginResultMessage)e.Message;

			var args = new ReceivedLoginEventArgs (msg.Result, msg.PlayerInfo);

			if (!msg.Result.Succeeded || (msg.Result.Succeeded && msg.PlayerInfo.Nickname == this.nickname))
			{
				this.userId = msg.Result.PlayerId;
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
	}
}