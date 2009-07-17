using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Gablarski.Media.Sources;
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

		protected void Setup (ClientUserManager userMananger, ClientChannelManager channelManager, ClientSourceManager sourceManager, CurrentUser currentUser)
		{
			if (this.handlers != null)
				return;

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
			lock (mqueue)
			{
				mqueue.Enqueue (e);
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
			this.Connection.IdentifyingTypes = new IdentifyingTypes (this.serverInfo.UserIdentifyingType, this.serverInfo.ChannelIdentifyingType);

			Trace.WriteLine ("[Client] Received server information: ");
			Trace.WriteLine ("Server name: " + this.serverInfo.ServerName);
			Trace.WriteLine ("Server description: " + this.serverInfo.ServerDescription);
			Trace.WriteLine ("User identifying type: " + this.serverInfo.UserIdentifyingType);
			Trace.WriteLine ("Channel identifying type: " + this.serverInfo.ChannelIdentifyingType);

			this.OnConnected (this, EventArgs.Empty);
		}
	}
}