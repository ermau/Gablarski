using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Gablarski.Media.Sources;
using Gablarski.Messages;

namespace Gablarski.Client
{
	public partial class GablarskiClient
	{
		public static readonly Version ApiVersion = Assembly.GetAssembly (typeof (GablarskiClient)).GetName ().Version;

		public GablarskiClient (IClientConnection connection)
			: this()
		{
			this.connection = connection;
			this.connection.Connected += this.OnConnected;
			this.connection.Disconnected += this.OnDisconnected;
		}

		#region Events
		public event EventHandler Connected;
		public event EventHandler<RejectedConnectionEventArgs> ConnectionRejected;
		public event EventHandler Disconnected;
		public event EventHandler<ReceivedLoginEventArgs> LoginResult;

		public event EventHandler<ReceivedListEventArgs<PlayerInfo>> ReceivedPlayerList;
		public event EventHandler<ReceivedLoginEventArgs> PlayerLoggedIn;
		public event EventHandler<PlayerDisconnectedEventArgs> PlayerDisconnected;

		public event EventHandler<ReceivedListEventArgs<Channel>> ReceivedChannelList;
		public event EventHandler<ChannelChangedEventArgs> PlayerChangedChannel;
		public event EventHandler<ChannelEditResultEventArgs> ReceivedChannelEditResult;
		
		public event EventHandler<ReceivedListEventArgs<MediaSourceInfo>> ReceivedSourceList;
		public event EventHandler<ReceivedSourceEventArgs> ReceivedSource;
		public event EventHandler<ReceivedAudioEventArgs> ReceivedAudioData;
		#endregion

		#region Public Properties
		public IMediaSource VoiceSource
		{
			get
			{
				IMediaSource source;
				lock (sourceLock)
				{
					this.clientSources.TryGetValue (MediaType.Voice, out source);
				}

				return source;
			}
		}

		public PlayerInfo Self
		{
			get
			{
				lock (this.players)
				{
					return this.players[this.playerId];
				}
			}
		}

		public IEnumerable<PlayerInfo> Players
		{
			get
			{
				lock (playerLock)
				{
					PlayerInfo[] playerCopy = new PlayerInfo[this.players.Count];
					this.players.Values.CopyTo (playerCopy, 0);

					return playerCopy;
				}
			}
		}

		public IEnumerable<Channel> Channels
		{
			get
			{
				lock (channelLock)
				{
					Channel[] channelCopy = new Channel[this.channels.Count];
					this.channels.Values.CopyTo (channelCopy, 0);

					return channelCopy;
				}
			}
		}
		#endregion

		#region Public Methods
		public void Connect (string host, int port)
		{
			if (host.IsEmpty ())
				throw new ArgumentException ("host must not be null or empty", "host");

			connection.MessageReceived += OnMessageReceived;
			connection.Connect (host, port);
			connection.Send (new ConnectMessage (ApiVersion));
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
			if (nickname.IsEmpty())
				throw new ArgumentException ("nickname must not be null or empty", "nickname");

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
			if (mediaSourceType == null)
				throw new ArgumentNullException ("mediaSourceType");
			if (mediaSourceType.GetInterface ("IMediaSource") == null)
				throw new InvalidOperationException ("Can not request a source that is not a media source.");

			lock (sourceLock)
			{
				if (this.clientSources.Values.Any (s => s.GetType () == mediaSourceType))
					throw new InvalidOperationException ("Client already owns a source of this type.");
			}

			this.connection.Send (new RequestSourceMessage (mediaSourceType, channels));
		}

		public void SendAudioData (Channel channel, IMediaSource source, byte[] data)
		{
			if (channel == null)
				throw new ArgumentNullException ("channel");
			if (source == null)
				throw new ArgumentNullException ("source");

			// TODO: Add bitrate transmision etc
			byte[] encoded = source.AudioCodec.Encode (data, source.AudioCodec.Bitrates.First(), source.AudioCodec.MaxQuality);
			this.connection.Send (new SendAudioDataMessage (channel.ChannelId, source.ID, encoded));
		}

		public void MovePlayerToChannel (PlayerInfo targetPlayer, Channel targetChannel)
		{
			if (targetPlayer == null)
				throw new ArgumentNullException ("targetPlayer");
			if (targetChannel == null)
				throw new ArgumentNullException ("targetChannel");

			this.connection.Send (new ChangeChannelMessage (targetPlayer.PlayerId, targetChannel.ChannelId));
		}

		public void CreateChannel (Channel channel)
		{
			if (channel == null)
				throw new ArgumentNullException ("channel");

			if (channel.ChannelId != 0)
				throw new ArgumentException ("Can not create an existing channel", "channel");

			this.connection.Send (new ChannelEditMessage (channel));
		}

		public void EditChannel (Channel channel)
		{
			if (channel == null)
				throw new ArgumentNullException ("channel");

			if (channel.ChannelId == 0)
				throw new ArgumentException ("channel must be an existing channel", "channel");

			this.connection.Send (new ChannelEditMessage (channel));
		}
		#endregion

		#region Event Invokers
		protected virtual void OnConnected (object sender, EventArgs e)
		{
			var connected = this.Connected;
			if (connected != null)
				connected (this, e);
		}

		protected virtual void OnDisconnected (object sender, EventArgs e)
		{
			var disconnected = this.Disconnected;
			if (disconnected != null)
				disconnected (this, e);
		}

		protected virtual void OnConnectionRejected (RejectedConnectionEventArgs e)
		{
			var rejected = this.ConnectionRejected;
			if (rejected != null)
				rejected (this, e);
		}

		protected virtual void OnPlayerDisconnected (PlayerDisconnectedEventArgs e)
		{
			var disconnected = this.PlayerDisconnected;
			if (disconnected != null)
				disconnected (this, e);
		}

		protected virtual void OnReceivedPlayerList (ReceivedListEventArgs<PlayerInfo> e)
		{
			var received = this.ReceivedPlayerList;
			if (received != null)
				received (this, e);
		}

		protected virtual void OnReceivedSourceList (ReceivedListEventArgs<MediaSourceInfo> e)
		{
			var received = this.ReceivedSourceList;
			if (received != null)
				received (this, e);
		}

		protected virtual void OnReceivedChannelList (ReceivedListEventArgs<Channel> e)
		{
			var received = this.ReceivedChannelList;
			if (received != null)
				received (this, e);
		}

		protected virtual void OnPlayerLoggedIn (ReceivedLoginEventArgs e)
		{
			var result = this.PlayerLoggedIn;
			if (result != null)
				result (this, e);
		}

		protected virtual void OnPlayerChangedChannnel (ChannelChangedEventArgs e)
		{
			var result = this.PlayerChangedChannel;
			if (result != null)
				result (this, e);
		}

		protected virtual void OnReceivedChannelEditResult (ChannelEditResultEventArgs e)
		{
			var received = this.ReceivedChannelEditResult;
			if (received != null)
				received (this, e);
		}

		protected virtual void OnLoginResult (ReceivedLoginEventArgs e)
		{
			var result = this.LoginResult;
			if (result != null)
				result (this, e);
		}

		protected virtual void OnReceivedSource (ReceivedSourceEventArgs e)
		{
			var received = this.ReceivedSource;
			if (received != null)
				received (this, e);
		}

		protected virtual void OnReceivedAudioData (ReceivedAudioEventArgs e)
		{
			var received = this.ReceivedAudioData;
			if (received != null)
				received (this, e);
		}
		#endregion
	}

	#region Event Args
	public class ChannelEditResultEventArgs
		: EventArgs
	{
		public ChannelEditResultEventArgs (Channel channel, ChannelEditResult result)
		{
			this.Channel = channel;
			this.Result = result;
		}

		public Channel Channel
		{
			get;
			private set;
		}

		public ChannelEditResult Result
		{
			get;
			private set;
		}
	}

	public class ChannelChangedEventArgs
		: EventArgs
	{
		public ChannelChangedEventArgs (ChannelChangeInfo moveInfo)
		{
			this.MoveInfo = moveInfo;
		}

		public ChannelChangeInfo MoveInfo
		{
			get;
			private set;
		}
	}

	public class RejectedConnectionEventArgs
		: EventArgs
	{
		public RejectedConnectionEventArgs (ConnectionRejectedReason reason)
		{
			this.Reason = reason;
		}

		public ConnectionRejectedReason Reason
		{
			get;
			private set;
		}
	}

	public class PlayerDisconnectedEventArgs
		: EventArgs
	{
		public PlayerDisconnectedEventArgs (PlayerInfo player)
		{
			this.Player = player;
		}

		public PlayerInfo Player
		{
			get;
			private set;
		}
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
	#endregion
}