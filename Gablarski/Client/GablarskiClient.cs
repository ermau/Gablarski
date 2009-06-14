using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Gablarski.Media.Sources;
using Gablarski.Messages;

namespace Gablarski.Client
{
	public partial class GablarskiClient
		: IClientContext
	{
		public static readonly Version ApiVersion = Assembly.GetAssembly (typeof (GablarskiClient)).GetName ().Version;

		public GablarskiClient (IClientConnection connection)
			: this (connection, new ClientChannelManager (connection), new ClientUserManager (connection), new ClientSourceManager (connection))
		{
		}

		public GablarskiClient (IClientConnection connection, ClientChannelManager channelManager, ClientUserManager userManager, ClientSourceManager sourceManager)
			: this (channelManager, userManager, sourceManager)
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
		public event EventHandler<ReceivedLoginResultEventArgs> ReceivedLoginResult;
		
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

		public MediaSourceBase VoiceSource
		{
			get
			{
				MediaSourceBase source;
				lock (sourceLock)
				{
					this.clientSources.TryGetValue (MediaType.Voice, out source);
				}

				return source;
			}
		}

		public ClientUserManager Users
		{
			get; 
			private set;
		}

		public ClientSourceManager Sources
		{
			get;
			private set;
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
			if (mediaSourceType.GetInterface ("MediaSourceBase") == null)
				throw new InvalidOperationException ("Can not request a source that is not a media source.");

			lock (sourceLock)
			{
				if (this.clientSources.Values.Any (s => s.GetType () == mediaSourceType))
					throw new InvalidOperationException ("Client already owns a source of this type.");
			}

			this.Connection.Send (new RequestSourceMessage (mediaSourceType, channels));
		}

		public void SendAudioData (Channel channel, MediaSourceBase source, byte[] data)
		{
			if (channel == null)
				throw new ArgumentNullException ("channel");
			if (source == null)
				throw new ArgumentNullException ("source");

			// TODO: Add bitrate transmision etc
			byte[] encoded = source.AudioCodec.Encode (data, 44100, source.AudioCodec.MaxQuality);
			this.Connection.Send (new SendAudioDataMessage (channel.ChannelId, source.Id, encoded));
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

		protected virtual void OnReceivedSourceList (ReceivedListEventArgs<MediaSourceInfo> e)
		{
			var received = this.ReceivedSourceList;
			if (received != null)
				received (this, e);
		}
	
		protected virtual void OnLoginResult (ReceivedLoginResultEventArgs e)
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

		#region IClientContext Members

		IClientConnection IClientContext.Connection
		{
			get { return this.Connection; }
		}

		IEnumerable<Channel> IClientContext.Channels
		{
			get { return this.Channels; }
		}

		IEnumerable<MediaSourceBase> IClientContext.Sources
		{
			get { return this.Sources; }
		}

		IEnumerable<ClientUser> IClientContext.Users
		{
			get { throw new NotImplementedException (); }
		}

		#endregion
	}

	#region Event Args
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
		public ReceivedAudioEventArgs (MediaSourceBase source, byte[] data)
		{
			this.Source = source;
			this.AudioData = data;
		}

		/// <summary>
		/// Gets the media source audio was received for.
		/// </summary>
		public MediaSourceBase Source
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

	public class ReceivedLoginResultEventArgs
		: EventArgs
	{
		public ReceivedLoginResultEventArgs (LoginResult result)
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
		public ReceivedSourceEventArgs (MediaSourceBase source, MediaSourceInfo sourceInfo, SourceResult result)
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

		public MediaSourceBase Source
		{
			get;
			set;
		}
	}
	#endregion
}