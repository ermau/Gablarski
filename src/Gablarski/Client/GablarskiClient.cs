// Copyright (c) 2010, Eric Maupin
// All rights reserved.
//
// Redistribution and use in source and binary forms, with
// or without modification, are permitted provided that
// the following conditions are met:
//
// - Redistributions of source code must retain the above 
//   copyright notice, this list of conditions and the
//   following disclaimer.
//
// - Redistributions in binary form must reproduce the above
//   copyright notice, this list of conditions and the
//   following disclaimer in the documentation and/or other
//   materials provided with the distribution.
//
// - Neither the name of Gablarski nor the names of its
//   contributors may be used to endorse or promote products
//   or services derived from this software without specific
//   prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS
// AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED
// WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
// WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR
// PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT
// HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT,
// INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
// (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE
// GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
// INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY,
// WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
// NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
// THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH
// DAMAGE.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Threading;
using Cadenza.Collections;
using Gablarski.Audio;
using Gablarski.Messages;
using Gablarski.Network;
using Cadenza;

namespace Gablarski.Client
{
	public partial class GablarskiClient
		: IClientContext
	{
		// ReSharper disable ConvertToConstant.Global
		public static readonly int ProtocolVersion = 6;
		// ReSharper restore ConvertToConstant.Global

		public GablarskiClient (IClientConnection connection)
			: this (connection, new AudioEngine())
		{
		}

		public GablarskiClient (IClientConnection connection, IAudioEngine audioEngine)
		{
			if (connection == null)
				throw new ArgumentNullException ("connection");
			if (audioEngine == null)
				throw new ArgumentNullException ("audioEngine");

			DebugLogging = Log.IsDebugEnabled;
			Setup (connection, audioEngine, null, null, null);
		}

		public GablarskiClient (IClientConnection connection, IAudioEngine audioEngine, IClientUserHandler userHandler, IClientSourceHandler sourceHandler, ClientChannelManager channelManager)
		{
			if (connection == null)
				throw new ArgumentNullException ("connection");
			if (audioEngine == null)
				throw new ArgumentNullException ("audioEngine");

			Setup (connection, audioEngine, userHandler, sourceHandler, channelManager);
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
		/// Permission was denied to requested action.
		/// </summary>
		public event EventHandler<PermissionDeniedEventArgs> PermissionDenied;

		/// <summary>
		/// The connection to the server has been lost (or forcibly closed.)
		/// </summary>
		public event EventHandler<DisconnectedEventArgs> Disconnected;

		#endregion

		#region Public Properties
		/// <summary>
		/// Gets whether the client is currently connected.
		/// </summary>
		public bool IsConnected
		{
			get { return (this.Connection.IsConnected && this.formallyConnected); }
		}

		public IClientConnection Connection
		{
			get; protected set;
		}

		/// <summary>
		/// Gets the channel manager for this client.
		/// </summary>
		public ClientChannelManager Channels
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets the user manager for this client.
		/// </summary>
		public IClientUserHandler Users
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets the source manager for this client.
		/// </summary>
		public IClientSourceHandler Sources
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets the audio engine responsible for playback and capture
		/// </summary>
		public IAudioEngine Audio
		{
			get; private set;
		}

		/// <summary>
		/// Gets the current user.
		/// </summary>
		public ICurrentUserHandler CurrentUser
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets the current channel the user is in.
		/// </summary>
		public ChannelInfo CurrentChannel
		{
			get { return this.Channels[this.CurrentUser.CurrentChannelId]; }
		}

		/// <summary>
		/// Gets the <see cref="ServerInfo"/> for the currently connected server. <c>null</c> if not connected.
		/// </summary>
		public ServerInfo ServerInfo
		{
			get { return this.serverInfo; }
		}

		private bool reconnectAutomatically = true;
		/// <summary>
		/// Gets or sets whether to reconnect automatically on disconnection. <c>true</c> by default.
		/// </summary>
		/// <seealso cref="ReconnectAttemptFrequency"/>
		public bool ReconnectAutomatically
		{
			get { return reconnectAutomatically; }
			set { reconnectAutomatically = value; }
		}

		private int reconnectAttemptFrequency = 2000;
		private bool formallyConnected;

		/// <summary>
		/// Gets or sets the frequency (ms) at which to attempt reconnection. (2s default).
		/// </summary>
		public int ReconnectAttemptFrequency
		{
			get { return reconnectAttemptFrequency; }
			set { reconnectAttemptFrequency = value; }
		}

		/// <summary>
		/// Gets or sets whether to trace verbosely.
		/// </summary>
		public bool VerboseTracing
		{
			get; set;
		}
		#endregion

		#region Public Methods
		/// <summary>
		/// Connects to a server server at <paramref name="host"/>:<paramref name="port"/>.
		/// </summary>
		/// <param name="host">The hostname of the server to connect to.</param>
		/// <param name="port">The port of the server to connect to.</param>
		/// <exception cref="ArgumentException"><paramref name="host"/> is invalid.</exception>
		/// <exception cref="System.ArgumentNullException"><paramref name="host"/> is <c>null</c>.</exception>
		/// <exception cref="System.ArgumentOutOfRangeException"><paramref name="port" /> is outside the acceptable port range.</exception>
		/// <exception cref="System.Net.Sockets.SocketException">Hostname could not be resolved.</exception>
		public void Connect (string host, int port)
		{
			ThreadPool.QueueUserWorkItem (o => ConnectCore (((Tuple<string, int>)o).Item1, ((Tuple<string, int>)o).Item2), new Tuple<string, int> (host, port));
		}

		/// <summary>
		/// Registers a message handler.
		/// </summary>
		/// <param name="messageType">The message type to register a handler for.</param>
		/// <param name="handler">The handler to register for the message type.</param>
		/// <exception cref="ArgumentNullException"><paramref name="handler"/> is <c>null</c>.</exception>
		/// <exception cref="InvalidOperationException"><paramref name="messageType"/> already has a registered handler.</exception>
		public void RegisterMessageHandler (ServerMessageType messageType, Action<MessageReceivedEventArgs> handler)
		{
			if (handler == null)
				throw new ArgumentNullException ("handler");

			this.handlers.Add (messageType, handler);
		}

		/// <summary>
		/// Disconnects from the current server.
		/// </summary>
		public void Disconnect()
		{
			ThreadPool.QueueUserWorkItem (s => DisconnectCore (DisconnectionReason.Requested, DisconnectHandling.None, this.Connection));
		}
		#endregion

		#region Event Invokers
		protected void OnConnected (object sender, EventArgs e)
		{
			var connected = this.Connected;
			if (connected != null)
				connected (this, e);
		}

		protected void OnDisconnected (object sender, DisconnectedEventArgs e)
		{
			var disconnected = this.Disconnected;
			if (disconnected != null)
				disconnected (this, e);
		}

		protected void OnConnectionRejected (RejectedConnectionEventArgs e)
		{
			DisconnectCore (DisconnectionReason.Unknown, DisconnectHandling.None, this.Connection, false);

			var rejected = this.ConnectionRejected;
			if (rejected != null)
				rejected (this, e);
		}

		protected void OnPermissionDenied (PermissionDeniedEventArgs e)
		{
			var denied = this.PermissionDenied;
			if (denied != null)
				denied (this, e);
		}
		#endregion

		IIndexedEnumerable<int, ChannelInfo> IClientContext.Channels
		{
			get { return this.Channels; }
		}

		private readonly MutableLookup<ServerMessageType, Action<MessageReceivedEventArgs>> handlers = new MutableLookup<ServerMessageType, Action<MessageReceivedEventArgs>>();
		private void Setup (IClientConnection connection, IAudioEngine audioEngine, IClientUserHandler userHandler, IClientSourceHandler sourceHandler, ClientChannelManager channelManager)
		{
			this.CurrentUser = new CurrentUser (this);

			this.Connection = connection;
			this.Audio = (audioEngine ?? new AudioEngine());
			this.Users = (userHandler ?? new ClientUserHandler (this, new ClientUserManager()));
			this.Sources = (sourceHandler ?? new ClientSourceHandler (this, new ClientSourceManager (this)));
			this.Channels = (channelManager ?? new ClientChannelManager (this));

			RegisterMessageHandler (ServerMessageType.PermissionDenied, OnPermissionDeniedMessage);
			RegisterMessageHandler (ServerMessageType.Redirect, OnRedirectMessage);
			RegisterMessageHandler (ServerMessageType.ServerInfoReceived, OnServerInfoReceivedMessage);
			RegisterMessageHandler (ServerMessageType.ConnectionRejected, OnConnectionRejectedMessage);
			RegisterMessageHandler (ServerMessageType.Disconnect, OnDisconnectedMessage);
		}

		protected ServerInfo serverInfo;

		protected readonly log4net.ILog Log = log4net.LogManager.GetLogger ("GablarskiClient");
		protected readonly bool DebugLogging;


		private int redirectLimit = 20;
		private int redirectCount;
		private volatile bool running;
		private readonly AutoResetEvent incomingWait = new AutoResetEvent (false);
		private readonly Queue<MessageReceivedEventArgs> mqueue = new Queue<MessageReceivedEventArgs> (100);
		private Thread messageRunnerThread;

		private IPEndPoint originalEndPoint;
		private int disconnectedInChannelId;

		private string originalHost;

		/*protected void Setup (ClientUserHandler userMananger, ClientChannelManager channelManager, ClientSourceHandler sourceHandler, CurrentUser currentUser, IAudioEngine audioEngine)
		{
			this.handlers = new Dictionary<ServerMessageType, Action<MessageReceivedEventArgs>>
			{
				{ ServerMessageType.PermissionDenied, OnPermissionDeniedMessage },

				{ ServerMessageType.Redirect, OnRedirectMessage },
				{ ServerMessageType.ServerInfoReceived, OnServerInfoReceivedMessage },
				
				{ ServerMessageType.ChannelList, this.Channels.OnChannelListReceivedMessage },
				{ ServerMessageType.ChannelEditResult, this.Channels.OnChannelEditResultMessage },

				{ ServerMessageType.ConnectionRejected, OnConnectionRejectedMessage },
				{ ServerMessageType.Disconnect, OnDisconnectedMessage },
				{ ServerMessageType.UserMuted, OnMuted },
			};
		}*/

		private void OnDisconnectedMessage (MessageReceivedEventArgs e)
		{
			var msg = (DisconnectMessage) e.Message;

			DisconnectHandling handling = DisconnectHandling.Reconnect;

			switch (msg.Reason)
			{
				case DisconnectionReason.LoggedInElsewhere:
					handling = DisconnectHandling.None;
					break;
			}

			DisconnectCore (msg.Reason, handling, e.Connection, true);
		}

		private void OnPermissionDeniedMessage (MessageReceivedEventArgs obj)
		{
			OnPermissionDenied (new PermissionDeniedEventArgs (((PermissionDeniedMessage)obj.Message).DeniedMessage));
		}

		private void MessageRunner ()
		{
			while (this.running)
			{
				while (mqueue.Count > 0)
				{
					MessageReceivedEventArgs e;
					lock (mqueue)
					{
						e = mqueue.Dequeue ();
					}

					var msg = (e.Message as ServerMessage);
					if (msg == null)
					{
						this.Log.Error ("Non ServerMessage received (" + e.Message.MessageTypeCode + "), disconnecting.");
						Connection.Disconnect ();
						return;
					}

					if (DebugLogging)
						this.Log.Debug ("Message Received: " + msg.MessageType);

					if (this.running)
					{
						IEnumerable<Action<MessageReceivedEventArgs>> mhandlers;
						if (this.handlers.TryGetValues (msg.MessageType, out mhandlers))
						{
							foreach (var h in mhandlers)
								h (e);
						}
					}
				}

				if (mqueue.Count == 0)
					incomingWait.WaitOne ();
			}
		}

		private void OnMessageReceived (object sender, MessageReceivedEventArgs e)
		{
			lock (mqueue)
			{
				mqueue.Enqueue (e);
			}

			incomingWait.Set ();
		}

		private void OnConnectionRejectedMessage (MessageReceivedEventArgs e)
		{
			var msg = (ConnectionRejectedMessage)e.Message;

			OnConnectionRejected (new RejectedConnectionEventArgs (msg.Reason));
		}

		private void OnServerInfoReceivedMessage (MessageReceivedEventArgs e)
		{
			this.serverInfo = ((ServerInfoMessage)e.Message).ServerInfo;
			this.Connection.Encryption = new Encryption (this.serverInfo.PublicRSAParameters);
			this.formallyConnected = true;

			OnConnected (this, EventArgs.Empty);
		}

		private void OnRedirectMessage (MessageReceivedEventArgs e)
		{
			var msg = (RedirectMessage) e.Message;

			int count = Interlocked.Increment (ref this.redirectCount);

			DisconnectCore (DisconnectionReason.Unknown, DisconnectHandling.None, this.Connection);
			
			if (count > redirectLimit)
				return;

			Connect (msg.Host, msg.Port);
		}

		private enum DisconnectHandling
		{
			None,
			Reconnect
		}

		private void DisconnectCore (DisconnectionReason reason, DisconnectHandling handling, IConnection connection)
		{
			DisconnectCore (reason, handling, connection, true);
		}

		private void DisconnectCore (DisconnectionReason reason, DisconnectHandling handling, IConnection connection, bool fireEvent)
		{
			disconnectedInChannelId = CurrentUser.CurrentChannelId;

			this.running = false;
			this.formallyConnected = false;

			connection.Disconnected -= this.OnDisconnectedInternal;
			connection.MessageReceived -= this.OnMessageReceived;

			this.incomingWait.Set ();

			lock (this.mqueue)
			{
				this.mqueue.Clear();
			}

			if (this.messageRunnerThread != null)
			{
				this.messageRunnerThread.Join ();
				this.messageRunnerThread = null;
			}

			if (fireEvent)
				OnDisconnected (this, new DisconnectedEventArgs (reason));
			
			this.Users.Reset();
			this.Channels.Clear();
			this.Sources.Reset();

			this.Audio.Stop();

			connection.Disconnect();

			if (handling == DisconnectHandling.Reconnect)
				ThreadPool.QueueUserWorkItem (Reconnect);
		}

		private void OnDisconnectedInternal (object sender, ConnectionEventArgs e)
		{
			DisconnectCore (DisconnectionReason.Unknown, (ReconnectAutomatically) ? DisconnectHandling.Reconnect : DisconnectHandling.None, e.Connection);
		}

		private void Reconnect (object state)
		{
			Thread.Sleep (ReconnectAttemptFrequency);

			CurrentUser.ReceivedJoinResult += ReconnectJoinedResult;
			ConnectCore();
		}

		private void ReconnectJoinedResult (object sender, ReceivedJoinResultEventArgs e)
		{
			if (e.Result != LoginResultState.Success)
				return;

			ChannelInfo channel = this.Channels[this.disconnectedInChannelId];
			if (channel != null)
				this.Users.Move (this.CurrentUser, channel);

			this.disconnectedInChannelId = 0;
		}

		private void ConnectCore (string host, int port)
		{
			if (host.IsNullOrWhitespace ())
				throw new ArgumentException ("host must not be null or empty", "host");

			try
			{
				this.originalHost = host;
				this.originalEndPoint = new IPEndPoint (Dns.GetHostAddresses (host).First (ip => ip.AddressFamily == AddressFamily.InterNetwork), port);
				ConnectCore();
			}
			catch (IOException)
			{
				OnConnectionRejected (new RejectedConnectionEventArgs (ConnectionRejectedReason.CouldNotConnect));
			}
			catch (SocketException)
			{
				OnConnectionRejected (new RejectedConnectionEventArgs (ConnectionRejectedReason.CouldNotConnect));
			}
		}

		private void ConnectCore ()
		{
			try
			{
				this.running = true;

				this.messageRunnerThread = new Thread (this.MessageRunner) { Name = "Gablarski Client Message Runner" };
				this.messageRunnerThread.SetApartmentState (ApartmentState.STA);
				this.messageRunnerThread.Priority = ThreadPriority.Highest;
				this.messageRunnerThread.Start ();

				this.Connection.Disconnected += this.OnDisconnectedInternal;
				this.Connection.MessageReceived += OnMessageReceived;
				this.Connection.Connect (this.originalEndPoint);
				this.Connection.Send (new ConnectMessage { ProtocolVersion = ProtocolVersion, Host = this.originalHost, Port = this.originalEndPoint.Port });

				if (Audio.AudioSender == null)
					Audio.AudioSender = Sources;
				if (Audio.AudioReceiver == null)
					Audio.AudioReceiver = Sources;

				this.Audio.Context = this;
				this.Audio.Start();
			}
			catch (SocketException)
			{
				OnConnectionRejected (new RejectedConnectionEventArgs (ConnectionRejectedReason.CouldNotConnect));
			}
		}

		#region Statics
		public static void QueryServer (IPEndPoint endpoint, IClientConnection connection)
		{
			connection.Connect (endpoint);
		}

		/// <summary>
		/// Searches for local servers and calls <paramref name="serversFound"/> for each server found.
		/// </summary>
		/// <param name="serversFound">Called each <paramref name="frequency"/> when servers return.</param>
		/// <param name="frequency">How many milliseconds between arrival of servers and the next query.</param>
		/// <param name="keepSearching">Returns <c>true</c> to keep searching, <c>false</c> to stop.</param>
		/// <exception cref="ArgumentNullException"><paramref name="serversFound"/> is <c>null</c>.</exception>
		public static void FindLocalServers (int frequency, Action<IEnumerable<Tuple<ServerInfo, IPEndPoint>>> serversFound, Func<bool> keepSearching)
		{
			if (serversFound == null)
				throw new ArgumentNullException ("serversFound");
			if (keepSearching == null)
				throw new ArgumentNullException ("keepSearching");

			Thread findthread = new Thread (FindLocalServersCore)
			{
				IsBackground = true,
				Name = "FindLocalServers"
			};
			findthread.Start (new Tuple<int, Action<IEnumerable<Tuple<ServerInfo, IPEndPoint>>>, Func<bool>> (frequency, serversFound, keepSearching));
		}

		private static void FindLocalServersCore (object o)
		{
			var found = (Tuple<int, Action<IEnumerable<Tuple<ServerInfo, IPEndPoint>>>, Func<bool>>)o;
			
			Socket s = new Socket (AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
			s.EnableBroadcast = true;
			s.Bind (new IPEndPoint (IPAddress.Any, 0));
			s.ReceiveTimeout = 1500;

			do
			{
				List<Tuple<QueryServerResultMessage, IPEndPoint>> results = new List<Tuple<QueryServerResultMessage, IPEndPoint>>();

				try
				{
					var msg = new QueryServerMessage { ServerInfoOnly = true };

					SocketValueWriter writer = new SocketValueWriter (s, new IPEndPoint (IPAddress.Broadcast, 42912));
					writer.WriteByte (42);
					writer.WriteUInt32 (0);
					writer.WriteUInt16 (msg.MessageTypeCode);
					msg.WritePayload (writer);
					writer.Flush();

					EndPoint server = new IPEndPoint (IPAddress.Any, 0);
					byte[] buffer = new byte[131072];
					ByteArrayValueReader reader = new ByteArrayValueReader (buffer);

					DateTime start = DateTime.Now;

					do
					{
						try
						{
							if (s.ReceiveFrom (buffer, ref server) == 0)
								continue;
						}
						catch (SocketException)
						{
							continue;
						}

						byte sanity = reader.ReadByte();
						if (sanity != 42)
							continue;

						ushort type = reader.ReadUInt16();

						var result = new QueryServerResultMessage();
						if (type != result.MessageTypeCode)
							continue;

						result.ReadPayload (reader);

						results.Add (new Tuple<QueryServerResultMessage,IPEndPoint> (result, (IPEndPoint)server));
					} while (DateTime.Now.Subtract (start).TotalSeconds < 2);
				}
				catch (Exception)
				{
					#if DEBUG
					throw;
					#endif
				}

				found.Item2 (results.Select (r => new Tuple<ServerInfo, IPEndPoint> (r.Item1.ServerInfo, r.Item2)));

				Thread.Sleep (found.Item1);
			} while (found.Item3());
		}
		#endregion
	}

	#region Event Args
	public class PermissionDeniedEventArgs
		: EventArgs
	{
		public PermissionDeniedEventArgs (ClientMessageType messageType)
		{
			this.DeniedMessage = messageType;
		}

		/// <summary>
		/// Gets the message type that was denied.
		/// </summary>
		public ClientMessageType DeniedMessage
		{
			get; private set;
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

	public class DisconnectedEventArgs
		: EventArgs
	{
		public DisconnectedEventArgs (DisconnectionReason reason)
		{
			Reason = reason;
		}

		protected DisconnectionReason Reason
		{
			get;
			private set;
		}
	}
	#endregion
}