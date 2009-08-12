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
				{ ServerMessageType.ServerInfoReceived, OnServerInfoReceivedMessage },
				{ ServerMessageType.UserListReceived, this.Users.OnUserListReceivedMessage },
				{ ServerMessageType.SourceListReceived, this.Sources.OnSourceListReceivedMessage },
				{ ServerMessageType.SourcesRemoved, this.Sources.OnSourcesRemovedMessage },
				
				{ ServerMessageType.ChannelListReceived, this.Channels.OnChannelListReceivedMessage },
				//{ ServerMessageType.ChangeChannelResult, OnChangeChannelResultMessage },
				{ ServerMessageType.UserChangedChannel, this.Users.OnUserChangedChannelMessage },
				{ ServerMessageType.ChannelEditResult, this.Channels.OnChannelEditResultMessage },

				{ ServerMessageType.ConnectionRejected, OnConnectionRejectedMessage },
				{ ServerMessageType.LoginResult, this.CurrentUser.OnLoginResultMessage },
				{ ServerMessageType.UserLoggedIn, this.Users.OnUserLoggedInMessage },
				{ ServerMessageType.UserDisconnected, this.Users.OnUserDisconnectedMessage },
				
				{ ServerMessageType.SourceResult, this.Sources.OnSourceResultMessage },
				{ ServerMessageType.AudioDataReceived, this.Sources.OnAudioDataReceivedMessage },
			};
		}

		private void MessageRunner ()
		{
			while (this.running)
			{
				if (mqueue.Count == 0)
				{
					Thread.Sleep (1);
					continue;
				}

				MessageReceivedEventArgs e;
				lock (mqueue)
				{
					e = mqueue.Dequeue ();
				}

				var msg = (e.Message as ServerMessage);
				if (msg == null)
				{
					Connection.Disconnect ();
					return;
				}

				Trace.WriteLineIf ((VerboseTracing || msg.MessageType != ServerMessageType.AudioDataReceived), "[Client] Message Received: " + msg.MessageType);

				this.handlers[msg.MessageType] (e);
			}
		}

		private void OnMessageReceived (object sender, MessageReceivedEventArgs e)
		{
			if (e.Message.MessageTypeCode != (ushort)ServerMessageType.AudioDataReceived)
			{
				lock (mqueue)
				{
					mqueue.Enqueue (e);
				}
			}
			else
			{
				this.handlers[(e.Message as ServerMessage).MessageType] (e);
			}
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
			Trace.WriteLine ("Server name: " + this.serverInfo.ServerName);
			Trace.WriteLine ("Server description: " + this.serverInfo.ServerDescription);

			this.OnConnected (this, EventArgs.Empty);
		}

		private enum DisconnectHandling
		{
			None,
			Reconnect
		}

		private void DisconnectCore (DisconnectHandling handling, IConnection connection)
		{
			if (!this.running)
				return;

			this.running = false;
			lock (this.mqueue)
			{
				this.mqueue.Clear();
			}

			connection.Disconnected -= this.OnDisconnectedInternal;
			connection.MessageReceived -= this.OnMessageReceived;
			connection.Disconnect();

			if (this.messageRunnerThread != null)
				this.messageRunnerThread.Join ();

			this.Users.Clear();
			this.Channels.Clear();
			this.Sources.Clear();

			OnDisconnected (this, EventArgs.Empty);
		}

		private void OnDisconnectedInternal (object sender, ConnectionEventArgs e)
		{
			DisconnectCore ((ReconnectAutomatically) ? DisconnectHandling.Reconnect : DisconnectHandling.None, e.Connection);
		}
	}
}