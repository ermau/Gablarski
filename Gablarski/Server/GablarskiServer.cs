using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gablarski.Client;
using Gablarski.Messages;
using System.Diagnostics;
using Gablarski.Media.Sources;

namespace Gablarski.Server
{
	public class GablarskiServer
	{
		public static readonly Version MinimumClientVersion = new Version (0,0,1,0);

		public GablarskiServer (ServerInfo serverInfo, IUserProvider userProvider)
		{
			this.serverInfo = serverInfo;
			this.userProvider = userProvider;

			this.Handlers = new Dictionary<ClientMessageType, Action<MessageReceivedEventArgs>>
			{
				{ ClientMessageType.Login, UserLoginAttempt },
				{ ClientMessageType.RequestSource, ClientRequestsSource },
				{ ClientMessageType.AudioData, AudioDataReceived }
			};
		}

		#region Public Methods
		public IEnumerable<IConnectionProvider> ConnectionProviders
		{
			get
			{
				lock (availableConnectionLock)
				{
					return this.availableConnections.ToArray ();
				}
			}

			set
			{
				this.availableConnections = value.ToList();
			}
		}

		public void AddConnectionProvider (IConnectionProvider connection)
		{
			Trace.WriteLine ("[Server] " + connection.GetType().Name + " added.");

			// MUST provide a gaurantee of persona
			connection.ConnectionMade += OnConnectionMade;
			connection.StartListening ();

			lock (availableConnectionLock)
			{
				this.availableConnections.Add (connection);
			}
		}

		public void RemoveConnectionProvider (IConnectionProvider connection)
		{
			Trace.WriteLine ("[Server] " + connection.GetType ().Name + " removed.");

			connection.StopListening ();
			connection.ConnectionMade -= this.OnConnectionMade;

			lock (availableConnectionLock)
			{
				this.availableConnections.Remove (connection);
			}
		}

		public void Disconnect (IConnection connection, string reason)
		{
			if (connection == null)
				throw new ArgumentNullException ("connection");

			connection.Disconnect ();
			connection.MessageReceived -= this.OnMessageReceived;
		}
		#endregion

		private readonly ServerInfo serverInfo = new ServerInfo();

		private readonly object availableConnectionLock = new object();
		private List<IConnectionProvider> availableConnections = new List<IConnectionProvider> ();
		
		private readonly IUserProvider userProvider;

		private object sourceLock = new object ();
		private readonly Dictionary<IConnection, List<IMediaSource>> sources = new Dictionary<IConnection, List<IMediaSource>> ();

		private readonly ConnectionCollection connections = new ConnectionCollection();

		private readonly Dictionary<ClientMessageType, Action<MessageReceivedEventArgs>> Handlers;

		protected virtual void OnMessageReceived (object sender, MessageReceivedEventArgs e)
		{
			var msg = (e.Message as ClientMessage);
			if (msg == null)
			{
				Disconnect (e.Connection, "Invalid message.");
				return;
			}

			Trace.WriteLine ("[Server] Message Received: " + msg.MessageType);

			this.Handlers[msg.MessageType] (e);
		}

		protected void AudioDataReceived (MessageReceivedEventArgs e)
		{
			var msg = (SendAudioDataMessage) e.Message;

			this.connections.Send (new AudioDataReceivedMessage (msg.SourceId, msg.Data), (IConnection c) => c != e.Connection);
		}

		protected void ClientRequestsSource (MessageReceivedEventArgs e)
		{
			var request = (RequestSourceMessage)e.Message;
			
			SourceResult result = SourceResult.FailedUnknown;
			int sourceId = -1;

			IMediaSource source = null;
			try
			{
				int index = 0;
				lock (sourceLock)
				{
					if (!sources.ContainsKey (e.Connection))
						sources.Add (e.Connection, new List<IMediaSource> ());

					if (!sources[e.Connection].Any (s => s != null && s.GetType () == request.MediaSourceType))
					{
						sourceId = sources.Sum (kvp => kvp.Value.Count);
						index = sources[e.Connection].Count;
						sources[e.Connection].Add (null);
					}
					else
						result = SourceResult.FailedPermittedSingleSourceOfType;
				}

				if (result == SourceResult.FailedUnknown)
				{
					source = MediaSources.Create (request.MediaSourceType, sourceId);
					if (source != null)
					{
						lock (sourceLock)
						{
							sources[e.Connection][index] = source;
						}

						result = SourceResult.Succeeded;
					}
					else
						result = SourceResult.FailedNotSupportedType;
				}
			}
			catch (OverflowException)
			{
				result = SourceResult.FailedLimit;
			}
			finally
			{
				MediaSourceInfo sourceInfo = new MediaSourceInfo
				                             	{
				                             		SourceId = sourceId,
													PlayerId = this.connections.GetPlayerId (e.Connection),
													MediaType = (source != null) ? source.Type : MediaType.None,
													SourceTypeName = request.MediaSourceType.AssemblyQualifiedName
				                             	};

				e.Connection.Send (new SourceResultMessage (result, sourceInfo));
				if (result == SourceResult.Succeeded)
				{
					connections.Send (new SourceResultMessage (SourceResult.NewSource, sourceInfo), (IConnection c) => c != e.Connection);
				}
			}
		}

		protected void UserLoginAttempt (MessageReceivedEventArgs e)
		{
			var login = (LoginMessage)e.Message;

			LoginResult result = this.userProvider.Login (login.Username, login.Password);
			PlayerInfo info = null;

			if (result.Succeeded)
			{
				info = new PlayerInfo (login.Nickname, result.PlayerId);

				this.connections.Add (e.Connection, info);
			}
			
			var msg = new LoginResultMessage (result, info);

			if (result.Succeeded)
				this.connections.Send (msg);
			else
				e.Connection.Send (msg);

			Trace.WriteLine ("[Server]" + login.Username + " Login: " + result.Succeeded + ". " + (result.FailureReason ?? String.Empty));
		}

		private void OnClientDisconnected (object sender, EventArgs e)
		{
			Trace.WriteLine ("[Server] Client disconnected");
		}
		
		private void OnConnectionMade (object sender, ConnectionEventArgs e)
		{
			Trace.WriteLine ("[Server] Connection Made");

			e.Connection.MessageReceived += this.OnMessageReceived;
			e.Connection.Disconnected += this.OnClientDisconnected;
			e.Connection.Send (new ServerInfoMessage (this.serverInfo));
			e.Connection.Send (new PlayerListMessage (this.connections.Players));

			IEnumerable<MediaSourceInfo> agrSources = Enumerable.Empty<MediaSourceInfo> ();
			lock (this.sourceLock)
			{
				foreach (var kvp in this.sources)
				{
					IConnection connection = kvp.Key;
					agrSources = agrSources.Concat (
							kvp.Value.Select (s => new MediaSourceInfo (s) { PlayerId = this.connections.GetPlayerId (connection) }));
				}

				agrSources = agrSources.ToList();
			}

			e.Connection.Send (new SourceListMessage (agrSources));
		}
	}
}