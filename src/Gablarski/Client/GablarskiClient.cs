// Copyright (c) 2011-2013, Eric Maupin
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
using System.Threading;
using System.Threading.Tasks;
using Cadenza.Collections;
using Gablarski.Audio;
using Gablarski.Messages;
using Tempest;
using Timer = System.Threading.Timer;

namespace Gablarski.Client
{
	public class GablarskiClient
		: TempestClient, IGablarskiClientContext
	{
		public GablarskiClient (IClientConnection connection)
			: this (connection, new AudioEngine())
		{
		}

		public GablarskiClient (IClientConnection connection, IAudioEngine audioEngine)
			: base (connection, MessageTypes.Reliable)
		{
			if (connection == null)
				throw new ArgumentNullException ("connection");
			if (audioEngine == null)
				throw new ArgumentNullException ("audioEngine");

			DebugLogging = Log.IsDebugEnabled;
			Setup (audioEngine, null, null, null);
		}

		public GablarskiClient (IClientConnection connection, IAudioEngine audioEngine, IClientUserHandler userHandler, IClientSourceHandler sourceHandler, ClientChannelManager channelManager)
			: base (connection, MessageTypes.Reliable)
		{
			if (connection == null)
				throw new ArgumentNullException ("connection");
			if (audioEngine == null)
				throw new ArgumentNullException ("audioEngine");

			Setup (audioEngine, userHandler, sourceHandler, channelManager);
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
		/// Gets whether the client is currently trying to connect or reconnect.
		/// </summary>
		public bool IsConnecting
		{
			get { return this.connecting; }
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
		public IChannelInfo CurrentChannel
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

		public override Task<ClientConnectionResult> ConnectAsync (Target target)
		{
			this.running = true;
			return base.ConnectAsync (target);
		}

		#region Event Invokers
		protected void OnConnected (object sender, EventArgs e)
		{
			this.connecting = false;
			this.pingTimer = new Timer (s => Connection.SendAsync (new ClientPingMessage()), null, 20000, 20000);
			Interlocked.Exchange (ref this.reconnectAttempt, 0);

			var connected = this.Connected;
			if (connected != null)
				connected (this, e);
		}

		protected void OnDisconnected (object sender, DisconnectedEventArgs e)
		{
			if (this.pingTimer != null)
				this.pingTimer.Dispose();

			var disconnected = this.Disconnected;
			if (disconnected != null)
				disconnected (this, e);
		}

		protected void OnConnectionRejected (RejectedConnectionEventArgs e)
		{
			this.connecting = false;
			e.Reconnect = (e.Reason == ConnectionRejectedReason.CouldNotConnect || e.Reason == ConnectionRejectedReason.Unknown);

			var rejected = this.ConnectionRejected;
			if (rejected != null)
				rejected (this, e);

			DisconnectCore (DisconnectionReason.Rejected, this.Connection, e.Reconnect, false);
		}

		protected void OnPermissionDenied (PermissionDeniedEventArgs e)
		{
			var denied = this.PermissionDenied;
			if (denied != null)
				denied (this, e);
		}
		#endregion

		IIndexedEnumerable<int, IChannelInfo> IGablarskiClientContext.Channels
		{
			get { return this.Channels; }
		}

		private void Setup (IAudioEngine audioEngine, IClientUserHandler userHandler, IClientSourceHandler sourceHandler, ClientChannelManager channelManager)
		{
			this.CurrentUser = new CurrentUser (this);

			this.Audio = (audioEngine ?? new AudioEngine());
			this.Users = (userHandler ?? new ClientUserHandler (this, new ClientUserManager()));
			this.Sources = (sourceHandler ?? new ClientSourceHandler (this, new ClientSourceManager (this)));
			this.Channels = (channelManager ?? new ClientChannelManager (this));

			this.RegisterMessageHandler<PermissionDeniedMessage> (OnPermissionDeniedMessage);
			this.RegisterMessageHandler<RedirectMessage> (OnRedirectMessage);
			this.RegisterMessageHandler<ServerInfoMessage> (OnServerInfoReceivedMessage);
			//this.RegisterMessageHandler<ConnectionRejectedMessage> (OnConnectionRejectedMessage);
			this.RegisterMessageHandler<DisconnectMessage> (OnDisconnectedMessage);
		}

		private Timer pingTimer;
		protected readonly object StateSync = new object();
		protected ServerInfo serverInfo;

		protected readonly log4net.ILog Log = log4net.LogManager.GetLogger ("GablarskiClient");
		protected readonly bool DebugLogging;

		private int redirectLimit = 20;
		private int redirectCount;

		private IPEndPoint originalEndPoint;
		private int disconnectedInChannelId;

		private string originalHost;
		private int reconnectAttempt;
		private bool connecting, running;

		private void OnDisconnectedMessage (MessageEventArgs<DisconnectMessage> e)
		{
			DisconnectCore (e.Message.Reason, (IClientConnection)e.Connection, (GetHandlingForReason (e.Message.Reason) == DisconnectHandling.Reconnect), true);
		}

		private void OnPermissionDeniedMessage (MessageEventArgs<PermissionDeniedMessage> e)
		{
			OnPermissionDenied (new PermissionDeniedEventArgs (e.Message.DeniedMessage));
		}

		private void OnServerInfoReceivedMessage (MessageEventArgs<ServerInfoMessage> e)
		{
			this.serverInfo = e.Message.ServerInfo;
			this.formallyConnected = true;

			OnConnected (this, EventArgs.Empty);
		}

		private void OnRedirectMessage (MessageEventArgs<RedirectMessage> e)
		{
			int count = Interlocked.Increment (ref this.redirectCount);

			DisconnectCore (DisconnectionReason.Redirected, this.Connection);
			
			if (count > redirectLimit)
				return;

			ConnectAsync (new Target (e.Message.Host, e.Message.Port));
		}

		private enum DisconnectHandling
		{
			None,
			Reconnect
		}

		private DisconnectHandling GetHandlingForReason (DisconnectionReason reason)
		{
			if (!ReconnectAutomatically)
				return DisconnectHandling.None;

			switch (reason)
			{
				case DisconnectionReason.Unknown:
					return DisconnectHandling.Reconnect;

				default:
					return DisconnectHandling.None;
			}
		}

		private void DisconnectCore (DisconnectionReason reason, IClientConnection connection)
		{
			DisconnectCore (reason, connection, GetHandlingForReason (reason) == DisconnectHandling.Reconnect, true);
		}

		private void DisconnectCore (DisconnectionReason reason, IClientConnection connection, bool reconnect, bool fireEvent)
		{
			lock (StateSync)
			{
				disconnectedInChannelId = CurrentUser.CurrentChannelId;

				this.connecting = false;
				this.running = false;
				this.formallyConnected = false;

				CurrentUser = new CurrentUser (this);
				this.Users.Reset();
				this.Channels.Clear();
				this.Sources.Reset();

				this.Audio.Stop();

				connection.DisconnectAsync();

				if (fireEvent)
					OnDisconnected (this, new DisconnectedEventArgs (reason));

				if (reconnect)
				{
					this.connecting = true;
					ThreadPool.QueueUserWorkItem (Reconnect);
				}
			}
		}

		private void OnDisconnectedInternal (object sender, ConnectionEventArgs e)
		{
			DisconnectCore (DisconnectionReason.Unknown, (IClientConnection)e.Connection);
		}

		private void Reconnect (object state)
		{
			Interlocked.Increment (ref this.reconnectAttempt);
			Thread.Sleep (ReconnectAttemptFrequency);

			lock (StateSync)
			{
				if (!this.running && !this.connecting)
					return;
			}

			CurrentUser.ReceivedJoinResult += ReconnectJoinedResult;
			//ConnectCore();
		}

		private void ReconnectJoinedResult (object sender, ReceivedJoinResultEventArgs e)
		{
			if (e.Result != LoginResultState.Success)
				return;

			IChannelInfo channel = this.Channels[this.disconnectedInChannelId];
			if (channel != null)
				this.Users.Move (this.CurrentUser, channel);

			this.disconnectedInChannelId = 0;
		}

		protected override void OnConnected (ClientConnectionEventArgs e)
		{
			lock (StateSync)
			{
				if (!this.running)
				{
					Connection.DisconnectAsync();
					return;
				}

				//this.Connection.Send (new ConnectMessage { ProtocolVersion = ProtocolVersion, Host = this.originalHost, Port = this.originalEndPoint.Port });

				if (Audio.AudioSender == null)
					Audio.AudioSender = Sources;
				if (Audio.AudioReceiver == null)
					Audio.AudioReceiver = Sources;

				Audio.Context = this;
				Audio.Start();
			}

			base.OnConnected(e);
		}
	}

	#region Event Args
	public class PermissionDeniedEventArgs
		: EventArgs
	{
		public PermissionDeniedEventArgs (GablarskiMessageType messageType)
		{
			DeniedMessage = messageType;
		}

		/// <summary>
		/// Gets the message type that was denied.
		/// </summary>
		public GablarskiMessageType DeniedMessage
		{
			get;
			private set;
		}
	}
	
	public class RejectedConnectionEventArgs
		: EventArgs
	{
		public RejectedConnectionEventArgs (ConnectionRejectedReason reason, int reconnectAttempt)
		{
			this.Reason = reason;
			ReconnectAttempt = reconnectAttempt;
			Reconnect = true;
		}

		/// <summary>
		/// Gets the reason for rejecting the connection.
		/// </summary>
		public ConnectionRejectedReason Reason
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets the reconnect attempt number. 0 = not an attempt.
		/// </summary>
		public int ReconnectAttempt
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets whether this was a reconnect attempt failing or not.
		/// </summary>
		public bool Reconnecting
		{
			get { return ReconnectAttempt != 0; }
		}

		/// <summary>
		/// Gets or sets whether to attempt to reconnect.
		/// </summary>
		public bool Reconnect
		{
			get;
			set;
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

	/// <summary>
	/// Holds data for the <see cref="GablarskiClient.Disconnected"/> event.
	/// </summary>
	public class DisconnectedEventArgs
		: EventArgs
	{
		public DisconnectedEventArgs (DisconnectionReason reason)
		{
			Reason = reason;
		}

		public DisconnectionReason Reason
		{
			get;
			private set;
		}
	}
	#endregion
}