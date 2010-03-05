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
using Cadenza;
using Gablarski.Audio;
using Gablarski.Messages;
using System.Threading;

namespace Gablarski.Client
{
	public partial class GablarskiClient
	{
		protected ServerInfo serverInfo;

		protected readonly log4net.ILog Log = log4net.LogManager.GetLogger ("GablarskiClient");
		protected readonly bool DebugLogging;

		protected Dictionary<ServerMessageType, Action<MessageReceivedEventArgs>> Handlers;
		protected internal IClientConnection Connection
		{
			get;
			private set;
		}

		private int redirectLimit = 20;
		private int redirectCount;
		private volatile bool running;
		private readonly AutoResetEvent incomingWait = new AutoResetEvent (false);
		private readonly Queue<MessageReceivedEventArgs> mqueue = new Queue<MessageReceivedEventArgs> (100);
		private Thread messageRunnerThread;

		private ClientUserHandler users;

		protected void Setup (ClientUserHandler userMananger, ClientChannelManager channelManager, ClientSourceManager sourceManager, CurrentUser currentUser, IAudioEngine audioEngine)
		{
			if (this.Handlers != null)
				return;

			this.Audio = (audioEngine ?? new AudioEngine());
			this.CurrentUser = currentUser;
			this.users = userMananger;
			this.Channels = channelManager;
			this.Sources = sourceManager;

			this.Handlers = new Dictionary<ServerMessageType, Action<MessageReceivedEventArgs>>
			{
				{ ServerMessageType.PermissionDenied, OnPermissionDeniedMessage },

				{ ServerMessageType.Redirect, OnRedirectReceived },
				{ ServerMessageType.ServerInfoReceived, OnServerInfoReceivedMessage },
				{ ServerMessageType.UserInfoList, this.users.OnUserListReceivedMessage },
				{ ServerMessageType.UserUpdated, this.users.OnUserUpdatedMessage },
				{ ServerMessageType.SourceList, this.Sources.OnSourceListReceivedMessage },
				{ ServerMessageType.SourcesRemoved, this.Sources.OnSourcesRemovedMessage },
				
				{ ServerMessageType.ChannelListReceived, this.Channels.OnChannelListReceivedMessage },
				{ ServerMessageType.UserChangedChannel, this.users.OnUserChangedChannelMessage },
				{ ServerMessageType.ChannelEditResult, this.Channels.OnChannelEditResultMessage },
				{ ServerMessageType.ChangeChannelResult, this.users.OnChannelChangeResultMessage },

				{ ServerMessageType.ConnectionRejected, OnConnectionRejectedMessage },
				{ ServerMessageType.LoginResult, this.CurrentUser.OnLoginResultMessage },
				{ ServerMessageType.JoinResult, this.CurrentUser.OnJoinResultMessage },
				{ ServerMessageType.Permissions, this.CurrentUser.OnPermissionsMessage },
				{ ServerMessageType.UserLoggedIn, this.users.OnUserJoinedMessage },
				{ ServerMessageType.UserDisconnected, this.users.OnUserDisconnectedMessage },
				{ ServerMessageType.Muted, OnMuted },
				
				{ ServerMessageType.SourceResult, this.Sources.OnSourceResultMessage },
				{ ServerMessageType.AudioData, this.Sources.OnAudioDataReceivedMessage },
				{ ServerMessageType.AudioSourceStateChange, this.Sources.OnAudioSourceStateChangedMessage },
			};
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
						Action<MessageReceivedEventArgs> handler;
						if (this.Handlers.TryGetValue (msg.MessageType, out handler))
							handler (e);
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

		private void OnMuted (MessageReceivedEventArgs obj)
		{
			var msg = (MutedMessage)obj.Message;

			if (msg.Type == MuteType.User)
				this.users.OnMutedMessage ((string)msg.Target, msg.Unmuted);
			else if (msg.Type == MuteType.AudioSource)
				this.Sources.OnMutedMessage ((int)msg.Target, msg.Unmuted);
		}

		private void OnConnectionRejectedMessage (MessageReceivedEventArgs e)
		{
			var msg = (ConnectionRejectedMessage)e.Message;

			OnConnectionRejected (new RejectedConnectionEventArgs (msg.Reason));
		}

		private void OnServerInfoReceivedMessage (MessageReceivedEventArgs e)
		{
			this.serverInfo = ((ServerInfoMessage)e.Message).ServerInfo;
			this.formallyConnected = true;

			OnConnected (this, EventArgs.Empty);
		}

		private void OnRedirectReceived (MessageReceivedEventArgs e)
		{
			var msg = (RedirectMessage) e.Message;

			int count = Interlocked.Increment (ref this.redirectCount);

			DisconnectCore (DisconnectHandling.None, this.Connection);
			
			if (count > redirectLimit)
				return;

			Connect (msg.Host, msg.Port);
		}

		private enum DisconnectHandling
		{
			None,
			Reconnect
		}

		private void DisconnectCore (DisconnectHandling handling, IConnection connection)
		{
			DisconnectCore (handling, connection, true);
		}

		private void DisconnectCore (DisconnectHandling handling, IConnection connection, bool fireEvent)
		{
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
				OnDisconnected (this, EventArgs.Empty);
			
			this.Users.Reset();
			this.Channels.Clear();
			this.Sources.Clear();

			this.Audio.Stop();

			connection.Disconnect();
		}

		private void OnDisconnectedInternal (object sender, ConnectionEventArgs e)
		{
			DisconnectCore ((ReconnectAutomatically) ? DisconnectHandling.Reconnect : DisconnectHandling.None, e.Connection);
		}

		private void ConnectCore (string host, int port)
		{
			if (host.IsNullOrWhitespace ())
				throw new ArgumentException ("host must not be null or empty", "host");

			try
			{
				IPEndPoint endPoint = new IPEndPoint (Dns.GetHostAddresses (host).First (ip => ip.AddressFamily == AddressFamily.InterNetwork), port);

				this.running = true;

				this.messageRunnerThread = new Thread (this.MessageRunner) { Name = "Gablarski Client Message Runner" };
				this.messageRunnerThread.SetApartmentState (ApartmentState.STA);
				this.messageRunnerThread.Priority = ThreadPriority.Highest;
				this.messageRunnerThread.Start ();

				this.Connection.Disconnected += this.OnDisconnectedInternal;
				this.Connection.MessageReceived += OnMessageReceived;
				this.Connection.Connect (endPoint);
				this.Connection.Send (new ConnectMessage { ProtocolVersion = ProtocolVersion, Host = host, Port = port });

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
	}
}