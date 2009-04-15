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

		public GablarskiServer (IUserProvider userProvider)
		{
			this.userProvider = userProvider;

			this.Handlers = new Dictionary<ClientMessageType, Action<MessageReceivedEventArgs>>
			{
				{ ClientMessageType.RequestToken, UserRequestsToken },
				{ ClientMessageType.Login, UserLoginAttempt },
				{ ClientMessageType.RequestSource, UserRequestsSource }
			};
		}

		#region Public Methods
		public IEnumerable<IConnectionProvider> ConnectionProviders
		{
			get
			{
				lock (connectionLock)
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

			lock (connectionLock)
			{
				this.availableConnections.Add (connection);
			}
		}

		public void RemoveConnectionProvider (IConnectionProvider connection)
		{
			Trace.WriteLine ("[Server] " + connection.GetType ().Name + " removed.");

			connection.StopListening ();
			connection.ConnectionMade -= this.OnConnectionMade;

			lock (connectionLock)
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

		private readonly object connectionLock = new object();
		private List<IConnectionProvider> availableConnections = new List<IConnectionProvider> ();
		
		private readonly IUserProvider userProvider;

		private object sourceLock = new object ();
		private readonly Dictionary<TokenedClient, List<IMediaSource>> sources = new Dictionary<TokenedClient, List<IMediaSource>> ();

		private readonly Dictionary<ClientMessageType, Action<MessageReceivedEventArgs>> Handlers;

		protected virtual void OnMessageReceived (object sender, MessageReceivedEventArgs e)
		{
			/* If a connection sends a garbage message or something invalid,
			 * the provider should 'send' a disconnected message. */
			var msg = (e.Message as ClientMessage);
			if (msg == null)
			{
				Disconnect (e.Connection, "Invalid message.");
				return;
			}

			Trace.WriteLine ("[Server] Message Received: " + msg.MessageType.ToString ());

			this.Handlers[msg.MessageType] (e);
		}

		protected void UserRequestsSource (MessageReceivedEventArgs e)
		{
			var request = (RequestSourceMessage)e.Message;

			SourceResult result = SourceResult.FailedUnknown;
			int sourceID = -1;

			var client = TokenedClient.GetTokenedClient (e.Connection);

			try
			{
				lock (sourceLock)
				{
					if (!sources.ContainsKey (client))
						sources.Add (client, new List<IMediaSource> ());

					sourceID = sources.Sum (kvp => kvp.Value.Count);
					sources[client].Add (null);
				}

				var source = MediaSources.Create (request.MediaSourceType, sourceID);
				if (source != null)
				{
					lock (sourceLock)
					{
						sources[client][sourceID] = source;
					}

					result = SourceResult.Succeeded;
				}
				else
					result = SourceResult.FailedNotSupportedType;
			}
			catch (OverflowException)
			{
				result = SourceResult.FailedLimit;
			}
			finally
			{
				e.Connection.Send (new SourceResultMessage (result, request.MediaSourceType) { SourceID = sourceID });
			}
		}

		protected void UserRequestsToken (MessageReceivedEventArgs e)
		{
			TokenedClient client = null;
			TokenResult result = TokenResult.FailedUnknown;

			var request = (RequestTokenMessage)e.Message;
			if (request.ClientVersion < MinimumClientVersion)
				result = TokenResult.FailedClientVersion;

			try
			{
				client = TokenedClient.GetTokenedClient (e.Connection);
				if (client != null)
					result = TokenResult.Succeeded;
			}
			catch (OverflowException)
			{
				result = TokenResult.FailedTokenOverflow;
			}
			finally
			{
				e.Connection.Send (new TokenResultMessage (result, (client != null) ? client.Token : 0));
			}
		}

		protected void UserLoginAttempt (MessageReceivedEventArgs e)
		{
			var login = (LoginMessage)e.Message;
			var client = TokenedClient.GetTokenedClient (login.Token);

			LoginResult result = null;
			if (client == null)
				result = new LoginResult (false, "Invalid token.");
			else
				result = this.userProvider.Login (login.Username, login.Password);

			e.Connection.Send (new LoginResultMessage (result));

			Trace.WriteLine ("[Server]" + login.Username + " Login: " + result.Succeeded + ". " + (result.FailureReason ?? String.Empty));

			if (!result.Succeeded)
				e.Connection.Disconnect ();
		}
		
		protected virtual void OnConnectionMade (object sender, ConnectionEventArgs e)
		{
			Trace.WriteLine ("[Server] Connection Made");

			e.Connection.MessageReceived += this.OnMessageReceived;
		}
	}
}