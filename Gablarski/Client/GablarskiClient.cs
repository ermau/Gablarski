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
	public partial class GablarskiClient
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

		public void Disconnect()
		{
			this.connection.Disconnect();
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
	}

	#region Event Args
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
	#endregion
}