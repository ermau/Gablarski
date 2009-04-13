using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gablarski.Client;
using Gablarski.Messages;
using System.Diagnostics;

namespace Gablarski.Server
{
	public class GablarskiServer
	{
		public GablarskiServer (IUserProvider userProvider)
		{
			this.userProvider = userProvider;

			this.Handlers = new Dictionary<ClientMessages, Action<MessageReceivedEventArgs>>
			{
				{ ClientMessages.RequestToken, UserRequestsToken },
				{ ClientMessages.Login, UserLoginAttempt }
			};
		}

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

		private readonly object connectionLock = new object();
		private List<IConnectionProvider> availableConnections = new List<IConnectionProvider> ();
		
		private readonly IUserProvider userProvider;

		private readonly Dictionary<ClientMessages, Action<MessageReceivedEventArgs>> Handlers;

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

		protected void UserRequestsToken (MessageReceivedEventArgs e)
		{
			e.Connection.Send (new TokenMessage (TokenedClient.GetTokenedClient (e.Connection).Token));
		}

		protected void UserLoginAttempt (MessageReceivedEventArgs e)
		{
			var login = (LoginMessage)e.Message;
			var result = this.userProvider.Login (login.Username, login.Password);

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