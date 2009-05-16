using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gablarski.Server;
using Gablarski.Network;
using Gablarski.Messages;
using System.Diagnostics;
using System.Reflection;
using Gablarski.Media.Sources;

namespace Gablarski.Client
{
	public class GablarskiClient
	{
		public static readonly Version ClientVersion = Assembly.GetAssembly (typeof (GablarskiClient)).GetName ().Version;

		protected GablarskiClient ()
		{
		}

		public GablarskiClient (IClientConnection connection)
		{
			this.connection = connection;
			this.connection.Connected += this.OnConnected;
			this.connection.Disconnected += this.OnDisconnected;

			this.Handlers = new Dictionary<ServerMessageType, Action<MessageReceivedEventArgs>>
			{
				{ ServerMessageType.ServerInfoReceived, OnServerInfoReceived },
				{ ServerMessageType.PlayerListReceived, OnPlayerListReceived },
				{ ServerMessageType.SourceListReceived, OnSourceListReceived },
				{ ServerMessageType.LoginResult, OnLoginResult },
				{ ServerMessageType.SourceResult, OnSourceReceived },
				{ ServerMessageType.AudioDataReceived, OnReceivedAudio }
			};
		}

		#region Events
		public event EventHandler Connected;
		public event EventHandler Disconnected;
		public event EventHandler<ReceivedLoginEventArgs> ReceivedLoginResult;
		public event EventHandler<ReceivedLoginEventArgs> ReceivedNewLogin;
		public event EventHandler<ReceivedSourceEventArgs> ReceivedSource;
		public event EventHandler<ReceivedAudioEventArgs> ReceivedAudioData;
		public event EventHandler<ReceivedListEventArgs<PlayerInfo>> ReceivedPlayerList;
		public event EventHandler<ReceivedListEventArgs<MediaSourceInfo>> ReceivedSourceList;
		#endregion

		public IMediaSource VoiceSource
		{
			get
			{
				IMediaSource source = null;
				lock (sourceLock)
				{
					this.clientSources.TryGetValue (MediaType.Voice, out source);
				}

				return source;
			}
		}

		#region Public Methods
		public void Connect (string host, int port)
		{
			connection.MessageReceived += OnMessageReceived;
			connection.Connect (host, port);
		}

		public void Login (string nickname)
		{
			Login (nickname, null, null);
		}

		public void Login (string nickname, string username, string password)
		{
			this.nickname = nickname;
			this.connection.Send (new LoginMessage
			{
				Nickname = nickname,
				Username = username,
				Password = password
			});
		}

		public void RequestSource (Type mediaSourceType, byte channels)
		{
			if (mediaSourceType.GetInterface ("IMediaSource") == null)
				throw new InvalidOperationException ("Can not request a source that is not a media source.");

			//lock (sourceLock)
			//{
			//    if (this.clientSources.Values.Any (s => s.GetType () == mediaSourceType))
			//        throw new InvalidOperationException ("Client already owns a source of this type.");
			//}

			this.connection.Send (new RequestSourceMessage (mediaSourceType, channels));
		}

		public void SendAudioData (IMediaSource source, byte[] data)
		{
			// TODO: Add bitrate transmision etc
			byte[] encoded = source.AudioCodec.Encode (data, source.AudioCodec.Bitrates.First(), source.AudioCodec.MaxQuality);
			this.connection.Send (new SendAudioDataMessage (source.ID, encoded));
		}
		#endregion

		#region Internals
		protected ServerInfo serverInfo;

		private string nickname;
		private long userId;

		protected readonly Dictionary<ServerMessageType, Action<MessageReceivedEventArgs>> Handlers;
		protected readonly IClientConnection connection;

		private object sourceLock = new object ();
		private Dictionary<MediaType, IMediaSource> clientSources = new Dictionary<MediaType, IMediaSource> ();
		private Dictionary<int, IMediaSource> allSources = new Dictionary<int, IMediaSource>();

		protected void AddSource (IMediaSource source, bool mine)
		{
			lock (sourceLock)
			{
				this.allSources.Add (source.ID, source);

				if (mine)
					this.clientSources.Add (source.Type, source);
			}
		}
		
		private void OnConnected (object sender, EventArgs e)
		{
			EventHandler connected = this.Connected;
			if (connected != null)
				connected (this, e);
		}

		private void OnDisconnected (object sender, EventArgs e)
		{
			EventHandler disconnected = this.Disconnected;
			if (disconnected != null)
				disconnected (this, e);
		}

		protected virtual void OnMessageReceived (object sender, MessageReceivedEventArgs e)
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

		protected void OnServerInfoReceived (MessageReceivedEventArgs e)
		{
			this.serverInfo = ((ServerInfoMessage)e.Message).ServerInfo;
			Trace.WriteLine ("[Client] Received server information: ");
			Trace.WriteLine ("Server name: " + this.serverInfo.ServerName);
			Trace.WriteLine ("Server description: " + this.serverInfo.ServerDescription);
		}

		private void OnPlayerListReceived (MessageReceivedEventArgs e)
		{
			var msg = (PlayerListMessage) e.Message;
			OnReceivedPlayerList (new ReceivedListEventArgs<PlayerInfo> (msg.Players));
		}

		protected virtual void OnReceivedPlayerList (ReceivedListEventArgs<PlayerInfo> e)
		{
			var received = this.ReceivedPlayerList;
			if (received != null)
				received (this, e);
		}

		private void OnSourceListReceived (MessageReceivedEventArgs e)
		{
			var msg = (SourceListMessage) e.Message;
			foreach (var sourceInfo in msg.Sources)
				this.AddSource (MediaSources.Create (Type.GetType (sourceInfo.SourceTypeName), sourceInfo.SourceId), sourceInfo.PlayerId == this.userId);

			OnReceivedSourceList (new ReceivedListEventArgs<MediaSourceInfo> (msg.Sources));
		}

		protected virtual void OnReceivedSourceList (ReceivedListEventArgs<MediaSourceInfo> e)
		{
			var received = this.ReceivedSourceList;
			if (received != null)
				received (this, e);
		}

		protected void OnLoginResult (MessageReceivedEventArgs e)
		{
			var msg = (LoginResultMessage) e.Message;

			var args = new ReceivedLoginEventArgs (msg.Result, msg.PlayerInfo);

			if (!msg.Result.Succeeded || (msg.Result.Succeeded && msg.PlayerInfo.Nickname == this.nickname))
			{
				this.userId = msg.Result.PlayerId;
				OnReceivedLoginResult (args);
			}
			else
				OnReceivedNewLogin (args);
		}

		protected virtual void OnReceivedNewLogin (ReceivedLoginEventArgs e)
		{
			var result = this.ReceivedNewLogin;
			if (result != null)
				result (this, e);
		}


		protected virtual void OnReceivedLoginResult (ReceivedLoginEventArgs e)
		{
			var result = this.ReceivedLoginResult;
			if (result != null)
				result (this, e);
		}

		protected void OnSourceReceived (MessageReceivedEventArgs e)
		{
			IMediaSource source = null;

			var sourceMessage = (SourceResultMessage)e.Message;
			if (sourceMessage.MediaSourceType == null)
			{
				Trace.WriteLine("[Client] Source type " + sourceMessage.SourceInfo.SourceTypeName + " not found.");
			}
			else if (sourceMessage.SourceResult == SourceResult.Succeeded || sourceMessage.SourceResult == SourceResult.NewSource)
			{
				source = sourceMessage.GetSource();
				this.AddSource (source, (sourceMessage.SourceResult == SourceResult.Succeeded));
			}

			OnReceivedSourceResult (new ReceivedSourceEventArgs (source, sourceMessage.SourceInfo, sourceMessage.SourceResult));
		}

		protected virtual void OnReceivedSourceResult (ReceivedSourceEventArgs e)
		{
			var received = this.ReceivedSource;
			if (received != null)
				received (this, e);
		}

		protected void OnReceivedAudio (MessageReceivedEventArgs e)
		{
			var msg = (AudioDataReceivedMessage) e.Message;
			OnReceivedAudioData (new ReceivedAudioEventArgs (this.allSources[msg.SourceId], msg.Data));
		}

		protected virtual void OnReceivedAudioData (ReceivedAudioEventArgs e)
		{
			var received = this.ReceivedAudioData;
			if (received != null)
				received (this, e);
		}
		#endregion
	}

	public class ReceivedListEventArgs<T>
		: EventArgs
	{
		public ReceivedListEventArgs (IEnumerable<T> data)
		{
			this.Data = data;
		}

		public IEnumerable<T> Data
		{
			get;
			private set;
		}
	}

	public class ReceivedAudioEventArgs
		: EventArgs
	{
		public ReceivedAudioEventArgs (IMediaSource source, byte[] data)
		{
			this.Source = source;
			this.AudioData = data;
		}

		public IMediaSource Source
		{
			get;
			set;
		}

		public byte[] AudioData
		{
			get;
			set;
		}
	}

	public class ReceivedLoginEventArgs
		: EventArgs
	{
		public ReceivedLoginEventArgs (LoginResult result, PlayerInfo info)
		{
			this.Result = result;
			this.PlayerInfo = info;
		}

		public PlayerInfo PlayerInfo
		{
			get;
			private set;
		}

		public LoginResult Result
		{
			get;
			private set;
		}
	}

	public class ReceivedSourceEventArgs
		: EventArgs
	{
		public ReceivedSourceEventArgs (IMediaSource source, MediaSourceInfo sourceInfo, SourceResult result)
		{
			this.Result = result;
			this.SourceInfo = sourceInfo;
			this.Source = source;
		}

		public SourceResult Result
		{
			get;
			set;
		}

		public MediaSourceInfo SourceInfo
		{
			get;
			set;
		}

		public IMediaSource Source
		{
			get;
			set;
		}
	}
}