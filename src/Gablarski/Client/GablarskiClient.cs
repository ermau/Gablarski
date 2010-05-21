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
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Threading;
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
			if (this.handlers.ContainsKey (messageType))
				throw new InvalidOperationException (messageType + " has already been registered.");

			this.handlers.Add (messageType, handler);
		}

		/// <summary>
		/// Disconnects from the current server.
		/// </summary>
		public void Disconnect()
		{
			ThreadPool.QueueUserWorkItem (s => DisconnectCore (DisconnectionReason.Unknown, DisconnectHandling.None, this.Connection));
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
			DisconnectHandling handling = DisconnectHandling.None;
			switch (e.Reason)
			{
				case ConnectionRejectedReason.CouldNotConnect:
				case ConnectionRejectedReason.Unknown:
					handling = DisconnectHandling.Reconnect;
					break;
			}

			DisconnectCore (DisconnectionReason.Unknown, handling, this.Connection, false);

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

		private readonly Dictionary<ServerMessageType, Action<MessageReceivedEventArgs>> handlers = new Dictionary<ServerMessageType, Action<MessageReceivedEventArgs>>();
		private void Setup (IClientConnection connection, IAudioEngine audioEngine, IClientUserHandler userHandler, IClientSourceHandler sourceHandler, ClientChannelManager channelManager)
		{
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