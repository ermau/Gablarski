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
		public event EventHandler<ReceivedLoginEventArgs> ReceivedLogin;
		public event EventHandler<ReceivedSourceEventArgs> ReceivedSource;
		public event EventHandler<ReceivedAudioEventArgs> ReceivedAudioData;
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

		protected void OnPlayerListReceived (MessageReceivedEventArgs e)
		{
			var msg = (PlayerListMessage) e.Message;
		}

		protected void OnSourceListReceived (MessageReceivedEventArgs e)
		{
			var msg = (SourceListMessage) e.Message;
		}

		protected void OnLoginResult (MessageReceivedEventArgs e)
		{
			var msg = (LoginResultMessage) e.Message;
			this.userId = msg.Result.PlayerId;

			OnReceivedLoginResult (new ReceivedLoginEventArgs (msg.Result));
		}

		protected virtual void OnReceivedLoginResult (ReceivedLoginEventArgs e)
		{
			var result = this.ReceivedLogin;
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

			OnReceivedSourceResult (new ReceivedSourceEventArgs (source, sourceMessage.SourceResult));
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
		public ReceivedLoginEventArgs (LoginResult result)
		{
			this.Result = result;
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
		public ReceivedSourceEventArgs (IMediaSource source, SourceResult result)
		{
			this.Result = result;
			this.Source = source;
		}

		public SourceResult Result
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