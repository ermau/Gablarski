// Copyright (c) 2009, Eric Maupin
// All rights reserved.

// Redistribution and use in source and binary forms, with
// or without modification, are permitted provided that
// the following conditions are met:

// - Redistributions of source code must retain the above 
//   copyright notice, this list of conditions and the
//   following disclaimer.

// - Redistributions in binary form must reproduce the above
//   copyright notice, this list of conditions and the
//   following disclaimer in the documentation and/or other
//   materials provided with the distribution.

// - Neither the name of Gablarski nor the names of its
//   contributors may be used to endorse or promote products
//   derived from this software without specific prior
//   written permission.

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
using System.Diagnostics;
using System.Linq;
using Gablarski.Audio;
using Gablarski.Messages;
using System.Threading;

namespace Gablarski.Client
{
	public partial class GablarskiClient
	{
		protected ServerInfo serverInfo;

		protected Dictionary<ServerMessageType, Action<MessageReceivedEventArgs>> handlers;
		protected internal IClientConnection Connection
		{
			get;
			private set;
		}

		private volatile bool running;
		private readonly AutoResetEvent incomingWait = new AutoResetEvent (false);
		private readonly Queue<MessageReceivedEventArgs> mqueue = new Queue<MessageReceivedEventArgs> (100);
		private Thread messageRunnerThread;

		protected void Setup (ClientUserManager userMananger, ClientChannelManager channelManager, ClientSourceManager sourceManager, CurrentUser currentUser, IAudioEngine audioEngine)
		{
			if (this.handlers != null)
				return;

			this.Audio = audioEngine;
			this.CurrentUser = currentUser;
			this.Users = userMananger;
			this.Channels = channelManager;
			this.Sources = sourceManager;

			this.handlers = new Dictionary<ServerMessageType, Action<MessageReceivedEventArgs>>
			{
				{ ServerMessageType.PermissionDenied, OnPermissionDeniedMessage },

				{ ServerMessageType.ServerInfoReceived, OnServerInfoReceivedMessage },
				{ ServerMessageType.UserListReceived, this.Users.OnUserListReceivedMessage },
				{ ServerMessageType.SourceListReceived, this.Sources.OnSourceListReceivedMessage },
				{ ServerMessageType.SourcesRemoved, this.Sources.OnSourcesRemovedMessage },
				
				{ ServerMessageType.ChannelListReceived, this.Channels.OnChannelListReceivedMessage },
				{ ServerMessageType.UserChangedChannel, this.Users.OnUserChangedChannelMessage },
				{ ServerMessageType.ChannelEditResult, this.Channels.OnChannelEditResultMessage },

				{ ServerMessageType.ConnectionRejected, OnConnectionRejectedMessage },
				{ ServerMessageType.LoginResult, this.CurrentUser.OnLoginResultMessage },
				{ ServerMessageType.JoinResult, this.CurrentUser.OnJoinResultMessage },
				{ ServerMessageType.Permissions, this.CurrentUser.OnPermissionsMessage },
				{ ServerMessageType.UserLoggedIn, this.Users.OnUserLoggedInMessage },
				{ ServerMessageType.UserDisconnected, this.Users.OnUserDisconnectedMessage },
				{ ServerMessageType.Muted, OnMuted },
				
				{ ServerMessageType.SourceResult, this.Sources.OnSourceResultMessage },
				{ ServerMessageType.AudioDataReceived, this.Sources.OnAudioDataReceivedMessage },
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
						e = mqueue.Dequeue();
					}

					var msg = (e.Message as ServerMessage);
					if (msg == null)
					{
						Trace.WriteLine ("[Client] Non ServerMessage received (" + e.Message.MessageTypeCode + "), disconnecting.");
						Connection.Disconnect();
						return;
					}

					Trace.WriteLineIf ((VerboseTracing || msg.MessageType != ServerMessageType.AudioDataReceived),
										"[Client] Message Received: " + msg.MessageType);

					if (this.running)
                        this.handlers[msg.MessageType] (e);
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
				this.Users.OnMutedMessage ((string)msg.Target, msg.Unmuted);
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

			Trace.WriteLine ("[Client] Received server information: ");
			Trace.WriteLine ("Server name: " + this.serverInfo.Name);
			Trace.WriteLine ("Server description: " + this.serverInfo.Description);

			this.OnConnected (this, EventArgs.Empty);
		}

		private enum DisconnectHandling
		{
			None,
			Reconnect
		}

		private void DisconnectCore (DisconnectHandling handling, IConnection connection)
		{
			this.running = false;

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

			OnDisconnected (this, EventArgs.Empty);
			
			connection.Disconnected -= this.OnDisconnectedInternal;
			connection.MessageReceived -= this.OnMessageReceived;
			connection.Disconnect();

			this.Users.Clear();
			this.Channels.Clear();
			this.Sources.Clear();

			this.Audio.Stop();
		}

		private void OnDisconnectedInternal (object sender, ConnectionEventArgs e)
		{
			DisconnectCore ((ReconnectAutomatically) ? DisconnectHandling.Reconnect : DisconnectHandling.None, e.Connection);
		}
	}
}