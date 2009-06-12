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
			: this (connection, new ClientChannelManager (connection))
		{
		}

		public GablarskiClient (IClientConnection connection, ClientChannelManager channelManager)
			: this (channelManager)
		{
			this.Connection = connection;
			this.Connection.Connected += this.OnConnected;
			this.Connection.Disconnected += this.OnDisconnected;
		}

		#region Events
		/// <summary>
		/// The client has connected to the server.
		/// </summary>
		public event EventHandler Connected;
		
		/// <summary>
		/// The connection to the server has been rejected.
		/// </summary>
		public event EventHandler<RejectedConnectionEventArgs> ConnectionRejected;

		/// <summary>
		/// The connection to the server has been lost (or forcibly closed.)
		/// </summary>
		public event EventHandler Disconnected;

		/// <summary>
		/// A login result has been received.
		/// </summary>
		public event EventHandler<ReceivedLoginEventArgs> ReceivedLoginResult;

		/// <summary>
		/// An new or updated player list has been received.
		/// </summary>
		public event EventHandler<ReceivedListEventArgs<UserInfo>> ReceivedPlayerList;

		/// <summary>
		/// A new player has logged in.
		/// </summary>
		public event EventHandler<ReceivedLoginEventArgs> PlayerLoggedIn;

		/// <summary>
		/// A player has disconnected.
		/// </summary>
		public event EventHandler<PlayerDisconnectedEventArgs> PlayerDisconnected;

		/// <summary>
		/// A player has changed channels.
		/// </summary>
		public event EventHandler<ChannelChangedEventArgs> PlayerChangedChannel;
		
		/// <summary>
		/// A new  or updated source list has been received.
		/// </summary>
		public event EventHandler<ReceivedListEventArgs<MediaSourceInfo>> ReceivedSourceList;

		/// <summary>
		/// A new media source has been received.
		/// </summary>
		public event EventHandler<ReceivedSourceEventArgs> ReceivedSource;

		/// <summary>
		/// Audio data has been received.
		/// </summary>
		public event EventHandler<ReceivedAudioEventArgs> ReceivedAudioData;
		#endregion

		#region Public Properties
		public ClientChannelManager Channels
		{
			get;
			private set;
		}

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

		/// <summary>
		/// Gets the current player's own <c>UserInfo</c>.
		/// </summary>
		public UserInfo Self
		{
			get
			{
				lock (this.players)
				{
					return this.players[this.playerId];
				}
			}
		}

		public IEnumerable<UserInfo> Players
		{
			get
			{
				lock (playerLock)
				{
					UserInfo[] playerCopy = new UserInfo[this.players.Count];
					this.players.Values.CopyTo (playerCopy, 0);

					return playerCopy;
				}
			}
		}
		#endregion

		#region Public Methods
		/// <summary>
		/// Connects to a server server at <paramref name="host"/>:<paramref name="port"/>.
		/// </summary>
		/// <param name="host">The hostname of the server to connect to.</param>
		/// <param name="port">The port of the server to connect to.</param>
		/// <exception cref="System.ArgumentNullException"><paramref name="host"/> is <c>null</c>.</exception>
		/// <exception cref="System.ArgumentOutOfRangeException"><paramref name="port" /> is outside the acceptable port range.</exception>
		public void Connect (string host, int port)
		{
			if (host.IsEmpty ())
				throw new ArgumentException ("host must not be null or empty", "host");

			this.running = true;
			this.messageRunnerThread.Start ();	

			Connection.MessageReceived += OnMessageReceived;
			Connection.Connect (host, port);
			Connection.Send (new ConnectMessage (ApiVersion));
		}

		/// <summary>
		/// Disconnects from the current server.
		/// </summary>
		public void Disconnect()
		{
			this.Connection.Disconnect();
			this.running = false;
			this.messageRunnerThread.Join ();
		}

		/// <summary>
		/// Logs into the connected server with <paramref name="nickname"/>.
		/// </summary>
		/// <param name="nickname">The nickname to log in with.</param>
		/// <exception cref="System.ArgumentNullException"><paramref name="nickname"/> is null or empty.</exception>
		public void Login (string nickname)
		{
			Login (nickname, null, null);
		}

		/// <summary>
		/// Logs into the connected server with <paramref name="nickname"/>.
		/// </summary>
		/// <param name="nickname">The nickname to log in with.</param>
		/// <param name="username">The username to log in with.</param>
		/// <param name="password">The password to log in with.</param>
		/// <exception cref="System.ArgumentNullException"><paramref name="nickname"/> is null or empty.</exception>
		public void Login (string nickname, string username, string password)
		{
			if (nickname.IsEmpty())
				throw new ArgumentNullException ("nickname", "nickname must not be null or empty");

			this.nickname = nickname;
			this.Connection.Send (new LoginMessage
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

			this.Connection.Send (new RequestSourceMessage (mediaSourceType, channels));
		}

		public void SendAudioData (Channel channel, IMediaSource source, byte[] data)
		{
			if (channel == null)
				throw new ArgumentNullException ("channel");
			if (source == null)
				throw new ArgumentNullException ("source");

			// TODO: Add bitrate transmision etc
			byte[] encoded = source.AudioCodec.Encode (data, 44100, source.AudioCodec.MaxQuality);
			this.Connection.Send (new SendAudioDataMessage (channel.ChannelId, source.ID, encoded));
		}

		public void MovePlayerToChannel (UserInfo targetPlayer, Channel targetChannel)
		{
			if (targetPlayer == null)
				throw new ArgumentNullException ("targetPlayer");
			if (targetChannel == null)
				throw new ArgumentNullException ("targetChannel");

			this.Connection.Send (new ChangeChannelMessage (targetPlayer.UserId, targetChannel.ChannelId));
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

		protected virtual void OnReceivedPlayerList (ReceivedListEventArgs<UserInfo> e)
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

		protected virtual void OnLoginResult (ReceivedLoginEventArgs e)
		{
			var result = this.ReceivedLoginResult;
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

		/// <summary>
		/// Gets the channel the edit request was for.
		/// </summary>
		public Channel Channel
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets the result of the channel edit request.
		/// </summary>
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

		/// <summary>
		/// Gets the move information.
		/// </summary>
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

		/// <summary>
		/// Gets the reason for rejecting the connection.
		/// </summary>
		public ConnectionRejectedReason Reason
		{
			get;
			private set;
		}
	}

	public class PlayerDisconnectedEventArgs
		: EventArgs
	{
		public PlayerDisconnectedEventArgs (UserInfo player)
		{
			this.Player = player;
		}

		/// <summary>
		/// Gets the player that disconnected.
		/// </summary>
		public UserInfo Player
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

		/// <summary>
		/// Gets the media source audio was received for.
		/// </summary>
		public IMediaSource Source
		{
			get;
			set;
		}

		/// <summary>
		/// Gets the audio data.
		/// </summary>
		public byte[] AudioData
		{
			get;
			set;
		}
	}

	public class ReceivedLoginEventArgs
		: EventArgs
	{
		public ReceivedLoginEventArgs (LoginResult result, UserInfo info)
		{
			this.Result = result;
			this.PlayerInfo = info;
		}

		/// <summary>
		/// Gets the information of the newly logged in player.
		/// </summary>
		public UserInfo PlayerInfo
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