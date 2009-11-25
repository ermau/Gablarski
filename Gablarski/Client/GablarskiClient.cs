// Copyright (c) 2009, Eric Maupin
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
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Threading;
using Gablarski.Audio;
using Gablarski.Messages;
using Gablarski.Network;
using Mono.Rocks;

namespace Gablarski.Client
{
	public partial class GablarskiClient
		: IClientContext
	{
		public static readonly int ProtocolVersion = 4;

		public GablarskiClient (IClientConnection connection)
			: this (connection, true)
		{
		}

		public GablarskiClient (IClientConnection connection, ClientUserManager userMananger, ClientChannelManager channelManager, ClientSourceManager sourceManager, CurrentUser currentUser)
			: this(connection, userMananger, channelManager, sourceManager, currentUser, new AudioEngine())
		{
		}

		public GablarskiClient (IClientConnection connection, ClientUserManager userMananger, ClientChannelManager channelManager, ClientSourceManager sourceManager, CurrentUser currentUser, IAudioEngine audioEngine)
			: this (connection, false)
		{
			Setup (userMananger, channelManager, sourceManager, currentUser, audioEngine);
		}

		private GablarskiClient (IClientConnection connection, bool setupDefaults)
		{
			this.Connection = connection;

			if (setupDefaults)
				Setup (new ClientUserManager (this), new ClientChannelManager (this), new ClientSourceManager (this), new CurrentUser (this), new AudioEngine());
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
		public event EventHandler Disconnected;

		#endregion

		#region Public Properties
		/// <summary>
		/// Gets whether the client is currently connected.
		/// </summary>
		public bool IsConnected
		{
			get { return this.Connection.IsConnected; }
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
		public ClientUserManager Users
		{
			get; 
			private set;
		}

		/// <summary>
		/// Gets the source manager for this client.
		/// </summary>
		public ClientSourceManager Sources
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
		public CurrentUser CurrentUser
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

		private int reconnectAttemptFrequency = 5000;
		/// <summary>
		/// Gets or sets the frequency (ms) at which to attempt reconnection. (5s default).
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
			if (host.IsEmpty ())
				throw new ArgumentException ("host must not be null or empty", "host");

			try
			{
				IPEndPoint endPoint = new IPEndPoint (Dns.GetHostAddresses (host).Where (ip => ip.AddressFamily == AddressFamily.InterNetwork).First(), port);

				this.running = true;

				this.messageRunnerThread = new Thread (this.MessageRunner) { Name = "Gablarski Client Message Runner" };
				this.messageRunnerThread.SetApartmentState (ApartmentState.STA);
				this.messageRunnerThread.Start();

				Connection.Disconnected += this.OnDisconnectedInternal;
				Connection.MessageReceived += OnMessageReceived;
				Connection.Connect (endPoint);
				Connection.Send (new ConnectMessage (ProtocolVersion));

				this.Audio.Context = this;
				this.Audio.AudioSender = this.Sources;
				this.Audio.AudioReceiver = this.Sources;
				this.Audio.Start();
			}
			catch (SocketException)
			{
				OnConnectionRejected (new RejectedConnectionEventArgs (ConnectionRejectedReason.CouldNotConnect));
			}
		}

		/// <summary>
		/// Disconnects from the current server.
		/// </summary>
		public void Disconnect()
		{
			DisconnectCore (DisconnectHandling.None, this.Connection);
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

		protected virtual void OnPermissionDenied (PermissionDeniedEventArgs e)
		{
			var denied = this.PermissionDenied;
			if (denied != null)
				denied (this, e);
		}
		#endregion

		#region IClientContext Members

		IClientConnection IClientContext.Connection
		{
			get { return this.Connection; }
		}

		IIndexedEnumerable<int, ChannelInfo> IClientContext.Channels
		{
			get { return this.Channels; }
		}

		IIndexedEnumerable<int, AudioSource> IClientContext.Sources
		{
			get { return this.Sources; }
		}

		IClientUserManager IClientContext.Users
		{
			get { return this.Users; }
		}
		#endregion

		#region Statics
		public static void QueryServer (IPEndPoint endpoint, IClientConnection connection)
		{
			connection.Connect (endpoint);
		}

		/// <summary>
		/// Searches for local servers and calls <paramref name="serverFound"/> for each server found.
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

			do
			{
				List<Tuple<QueryServerResultMessage, IPEndPoint>> results = new List<Tuple<QueryServerResultMessage, IPEndPoint>>();

				try
				{
					Socket s = new Socket (AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
					s.EnableBroadcast = true;
					s.Bind (new IPEndPoint (IPAddress.Any, 0));
					s.ReceiveTimeout = 1500;

					var msg = new QueryServerMessage { ServerInfoOnly = true };

					SocketValueWriter writer = new SocketValueWriter (s, new IPEndPoint (IPAddress.Broadcast, 6112));
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

				found._2 (results.Select (r => new Tuple<ServerInfo, IPEndPoint> (r._1.ServerInfo, r._2)));

				Thread.Sleep (found._1);
			} while (found._3());
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
	#endregion
}