//
// GablarskiClient.cs
//
// Author:
//   Eric Maupin <me@ermau.com>
//
// Copyright (c) 2009-2011, Eric Maupin
// Copyright (c) 2011-2014, Xamarin Inc.
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
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Gablarski.Audio;
using Gablarski.Messages;
using Tempest;
using Tempest.Providers.Network;

namespace Gablarski.Client
{
	public class GablarskiClient
		: IGablarskiClientContext
	{
		public static async Task<QueryResults> QueryAsync (RSAAsymmetricKey key, Target target, TimeSpan timeout)
		{
			if (key == null)
				throw new ArgumentNullException ("key");
			if (target == null)
				throw new ArgumentNullException ("target");

			var tcs = new TaskCompletionSource<QueryResults>();

			var connection = new UdpClientConnection (GablarskiProtocol.Instance, key);
			connection.Start (MessageTypes.Unreliable);

			var cancelSources = new CancellationTokenSource (timeout);
			cancelSources.Token.Register (() => {
				tcs.TrySetCanceled();
				connection.Dispose();
			});

			connection.ConnectionlessMessageReceived += (sender, args) => {
				var results = args.Message as QueryServerResultMessage;
				if (results == null)
					return;

				tcs.TrySetResult (new QueryResults (results.ServerInfo, results.Channels, results.Users));

				connection.Dispose();
			};

			await connection.SendConnectionlessMessageAsync (new QueryServerMessage(), target).ConfigureAwait (false);

			return await tcs.Task.ConfigureAwait (false);
		}

		public GablarskiClient (RSAAsymmetricKey key)
		{
			if (key == null)
				throw new ArgumentNullException ("key");

			Setup (new UdpClientConnection (GablarskiProtocol.Instance, key), null, null, null, null);
		}

		public event PropertyChangedEventHandler PropertyChanged;

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

		/// <summary>
		/// Gets whether the client is currently trying to connect or reconnect.
		/// </summary>
		public bool IsConnecting
		{
			get { return this.connecting; }
			private set
			{
				if (this.connecting == value)
					return;

				this.connecting = value;
				OnPropertyChanged();
			}
		}

		public bool IsConnected
		{
			get { return this.formallyConnected; }
			private set
			{
				if (this.formallyConnected == value)
					return;

				this.formallyConnected = value;
				OnPropertyChanged();
			}
		}

		/// <summary>
		/// Gets the channel manager for this client.
		/// </summary>
		public IClientChannelHandler Channels
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
			get;
			private set;
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
		/// Gets the <see cref="ServerInfo"/> for the currently connected server. <c>null</c> if not connected.
		/// </summary>
		public ServerInfo ServerInfo
		{
			get { return this.serverInfo; }
			private set
			{
				if (this.serverInfo == value)
					return;

				this.serverInfo = value;
				OnPropertyChanged();
			}
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

		/// <summary>
		/// Gets or sets the frequency (ms) at which to attempt reconnection. (2s default).
		/// </summary>
		public int ReconnectAttemptFrequency
		{
			get { return reconnectAttemptFrequency; }
			set { reconnectAttemptFrequency = value; }
		}

		public async Task<ClientConnectionResult> ConnectAsync (Target target)
		{
			var tcs = new TaskCompletionSource<ClientConnectionResult>();
			var oldTcs = Interlocked.Exchange (ref this.connectTcs, tcs);
			if (oldTcs != null)
				oldTcs.TrySetCanceled();

			this.previousTarget = target;
			this.running = true;
			ClientConnectionResult result = await this.client.ConnectAsync (target).ConfigureAwait (false);
			if (result.Result != ConnectionResult.Success)
				return result;

			if (!this.running) {
				await this.client.DisconnectAsync().ConfigureAwait (false);
				return result;
			}

			await client.Connection.SendAsync (new ConnectMessage {
				ProtocolVersion = 14,
			}).ConfigureAwait (false);

			lock (StateSync) {
				if (Audio.AudioSender == null)
					Audio.AudioSender = Sources;
				if (Audio.AudioReceiver == null)
					Audio.AudioReceiver = Sources;

				Audio.Context = this;
				Audio.Start();
			}

			return await tcs.Task.ConfigureAwait (false);
		}

		public Task DisconnectAsync()
		{
			return this.client.DisconnectAsync();
		}

		private void Setup (IClientConnection connection, IAudioEngine audioEngine, IClientUserHandler userHandler, IClientSourceHandler sourceHandler, ClientChannelHandler channelHandler)
		{
			this.client = new TempestClient (connection, MessageTypes.All);
			this.client.Disconnected += OnClientDisconnected;

			CurrentUser = new CurrentUser (this);

			Audio = (audioEngine ?? new AudioEngine());
			Users = (userHandler ?? new ClientUserHandler (this));
			Sources = (sourceHandler ?? new ClientSourceHandler (this));
			Channels = (channelHandler ?? new ClientChannelHandler (this));

			this.RegisterMessageHandler<PermissionDeniedMessage> (OnPermissionDeniedMessage);
			//this.RegisterMessageHandler<RedirectMessage> (OnRedirectMessage);
			this.RegisterMessageHandler<ServerInfoMessage> (OnServerInfoReceivedMessage);
			//this.RegisterMessageHandler<ConnectionRejectedMessage> (OnConnectionRejectedMessage);
			this.RegisterMessageHandler<DisconnectMessage> (OnDisconnectedMessage);
		}

		private Target previousTarget;

		protected readonly object StateSync = new object();
		private ServerInfo serverInfo;

		protected readonly bool DebugLogging;

		private int redirectLimit = 20;
		private int redirectCount;

		private IPEndPoint originalEndPoint;
		private int disconnectedInChannelId;

		private string originalHost;
		private bool formallyConnected;
		private int reconnectAttemptFrequency = 2000;
		private int reconnectAttempt;
		private bool connecting, running;
		private TaskCompletionSource<ClientConnectionResult> connectTcs;

		private TempestClient client;

		protected virtual void OnPropertyChanged ([CallerMemberName] string propertyName = null)
		{
			var changed = PropertyChanged;
			if (changed != null)
				changed (this, new PropertyChangedEventArgs (propertyName));
		}

		protected void OnConnected (object sender, EventArgs e)
		{
			this.connecting = false;
			Interlocked.Exchange (ref this.reconnectAttempt, 0);

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
			this.connecting = false;

			var rejected = this.ConnectionRejected;
			if (rejected != null)
				rejected (this, e);

			DisconnectCore (DisconnectionReason.Rejected, this.client.Connection, false);
		}

		protected void OnPermissionDenied (PermissionDeniedEventArgs e)
		{
			var denied = this.PermissionDenied;
			if (denied != null)
				denied (this, e);
		}

		private void OnDisconnectedMessage (MessageEventArgs<DisconnectMessage> e)
		{
			DisconnectCore (e.Message.Reason, (IClientConnection)e.Connection, true);
		}

		private void OnPermissionDeniedMessage (MessageEventArgs<PermissionDeniedMessage> e)
		{
			OnPermissionDenied (new PermissionDeniedEventArgs (e.Message.DeniedMessage));
		}

		private void OnServerInfoReceivedMessage (MessageEventArgs<ServerInfoMessage> e)
		{
			ServerInfo = e.Message.ServerInfo;
			IsConnected = true;

			var tcs = Interlocked.Exchange (ref this.connectTcs, null);
			if (tcs != null)
				tcs.TrySetResult (new ClientConnectionResult (ConnectionResult.Success, this.client.Connection.RemoteKey));

			OnConnected (this, EventArgs.Empty);
		}

		private void OnRedirectMessage (MessageEventArgs<RedirectMessage> e)
		{
			int count = Interlocked.Increment (ref this.redirectCount);

			DisconnectCore (DisconnectionReason.Redirected, this.client.Connection);
			
			if (count > redirectLimit)
				return;

			ConnectAsync (new Target (e.Message.Host, e.Message.Port));
		}

		private void DisconnectCore (DisconnectionReason reason, IClientConnection connection, bool fireEvent = true)
		{
			lock (StateSync)
			{
				disconnectedInChannelId = CurrentUser.CurrentChannelId;

				IsConnecting = false;
				this.running = false;
				IsConnected = false;

				CurrentUser = new CurrentUser (this);
				Users.Reset();
				Channels.Reset();
				Sources.Reset();

				Audio.Stop();

				connection.DisconnectAsync();

				if (fireEvent)
					OnDisconnected (this, new DisconnectedEventArgs (reason));
			}
		}

		private void OnClientDisconnected (object sender, ClientDisconnectedEventArgs e)
		{
			DisconnectCore (DisconnectionReason.Unknown, this.client.Connection);
		}

		void IContext.LockHandlers()
		{
			this.client.LockHandlers();
		}

		void IContext.RegisterConnectionlessMessageHandler (Protocol protocol, ushort messageType, Action<ConnectionlessMessageEventArgs> handler)
		{
			this.client.RegisterConnectionlessMessageHandler (protocol, messageType, handler);
		}

		void IContext.RegisterMessageHandler (Protocol protocol, ushort messageType, Action<MessageEventArgs> handler)
		{
			this.client.RegisterMessageHandler (protocol, messageType, handler);
		}

		IClientConnection IClientContext.Connection
		{
			get { return this.client.Connection; }
		}
	}

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
			Reason = reason;
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
}