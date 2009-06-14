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
		protected GablarskiClient (ClientChannelManager channelManager, ClientUserManager userManager, ClientSourceManager sourceManager)
		{
			this.Channels = channelManager;
			this.Users = userManager;

			this.Handlers = new Dictionary<ServerMessageType, Action<MessageReceivedEventArgs>>
			{
				{ ServerMessageType.ServerInfoReceived, OnServerInfoReceivedMessage },
				{ ServerMessageType.UserListReceived, this.Users.OnUserListReceivedMessage },
				//{ ServerMessageType.SourceListReceived, OnSourceListReceivedMessage },
				
				{ ServerMessageType.ChannelListReceived, this.Channels.OnChannelListReceivedMessage },
				//{ ServerMessageType.ChangeChannelResult, OnChangeChannelResultMessage },
				{ ServerMessageType.ChannelEditResult, this.Channels.OnChannelEditResultMessage },

				{ ServerMessageType.ConnectionRejected, OnConnectionRejectedMessage },
				//{ ServerMessageType.LoginResult, OnLoginResultMessage },
				{ ServerMessageType.UserLoggedIn, this.Users.OnUserLoggedInMessage },
				{ ServerMessageType.UserDisconnected, this.Users.OnUserDisconnectedMessage },
				
				//{ ServerMessageType.SourceResult, OnSourceResultMessage },
				{ ServerMessageType.AudioDataReceived, OnAudioDataReceivedMessage },
			};

			this.messageRunnerThread = new Thread (this.MessageRunner);
			this.messageRunnerThread.Name = "Gablarski Client Message Runner";
			this.messageRunnerThread.SetApartmentState (ApartmentState.STA);
		}
		
		protected ServerInfo serverInfo;

		private string nickname;
		private object playerId;

		protected readonly Dictionary<ServerMessageType, Action<MessageReceivedEventArgs>> Handlers;
		protected internal IClientConnection Connection
		{
			get;
			private set;
		}

		private object sourceLock = new object ();
		private Dictionary<MediaType, MediaSourceBase> clientSources = new Dictionary<MediaType, MediaSourceBase> ();
		private Dictionary<int, MediaSourceBase> allSources = new Dictionary<int, MediaSourceBase> ();

		private object channelLock = new object();
		private Dictionary<object, Channel> channels = new Dictionary<object, Channel> ();

		private void AddSource (MediaSourceBase source, bool mine)
		{
			lock (sourceLock)
			{
				this.allSources.Add (source.Id, source);

				if (mine)
					this.clientSources.Add (source.Type, source);
			}
		}

		private volatile bool running;
		private readonly Queue<MessageReceivedEventArgs> mqueue = new Queue<MessageReceivedEventArgs> (100);
		private readonly Thread messageRunnerThread;

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

				Trace.WriteLineIf ((msg.MessageType != ServerMessageType.AudioDataReceived), "[Client] Message Received: " + msg.MessageType);

				if (msg.MessageType == ServerMessageType.AudioDataReceived)
				{
					var amsg = (AudioDataReceivedMessage)msg;
					var received = this.ReceivedAudioData;
					if (received != null)
						received (this, new ReceivedAudioEventArgs (this.allSources[amsg.SourceId], amsg.Data));
				}
				else
					this.Handlers[msg.MessageType] (e);
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
			Trace.WriteLine ("[Client] Received server information: ");
			Trace.WriteLine ("Server name: " + this.serverInfo.ServerName);
			Trace.WriteLine ("Server description: " + this.serverInfo.ServerDescription);
		}

		

		#region Sources
		//private void OnSourceListReceivedMessage (MessageReceivedEventArgs e)
		//{
		//    var msg = (SourceListMessage)e.Message;
		//    foreach (var sourceInfo in msg.Sources)
		//        this.AddSource (MediaSources.Create (Type.GetType (sourceInfo.SourceTypeName), sourceInfo.SourceId, sourceInfo.UserId), sourceInfo.UserId == this.Self.UserId);

		//    OnReceivedSourceList (new ReceivedListEventArgs<MediaSourceInfo> (msg.Sources));
		//}

		//private void OnSourceResultMessage (MessageReceivedEventArgs e)
		//{
		//    MediaSourceBase source = null;

		//    var sourceMessage = (SourceResultMessage)e.Message;
		//    if (sourceMessage.MediaSourceType == null)
		//    {
		//        Trace.WriteLine ("[Client] Source type " + sourceMessage.SourceInfo.SourceTypeName + " not found.");
		//    }
		//    else if (sourceMessage.SourceResult == SourceResult.Succeeded || sourceMessage.SourceResult == SourceResult.NewSource)
		//    {
		//        source = sourceMessage.GetSource (this.Self.UserId);
		//        this.AddSource (source, (sourceMessage.SourceResult == SourceResult.Succeeded));
		//    }

		//    OnReceivedSource (new ReceivedSourceEventArgs (source, sourceMessage.SourceInfo, sourceMessage.SourceResult));
		//}

		private void OnAudioDataReceivedMessage (MessageReceivedEventArgs e)
		{
			var msg = (AudioDataReceivedMessage)e.Message;
			OnReceivedAudioData (new ReceivedAudioEventArgs (this.allSources[msg.SourceId], msg.Data));
		}
		#endregion
	}
}