using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using Gablarski.Messages;

namespace Gablarski
{
	public class TokenedClient
	{
		internal TokenedClient (int token, IConnection connection)
		{
			this.Token = token;
			this.Connection = connection;
			this.Authenticated = DateTime.Now;
		}

		internal TokenedClient (TokenedClient client)
			: this (client.Token, client.Connection)
		{
		}

		public bool Expired
		{
			get { return DateTime.Now.Subtract (this.Authenticated).TotalSeconds > TokenLifetime; }
		}

		public DateTime Authenticated
		{
			get;
			set;
		}

		public int Token
		{
			get;
			private set;
		}

		public IConnection Connection
		{
			get;
			private set;
		}

		public void Send (MessageBase message)
		{
			this.Connection.Send (message);
		}

		public bool GetExpiredOrRefresh ()
		{
			if (!this.Expired)
				this.Authenticated = DateTime.Now;

			return this.Expired;
		}

		private static object connectionLock = new object ();
		private static readonly Dictionary<IConnection, TokenedClient> connections = new Dictionary<IConnection, TokenedClient> ();
		private static readonly Dictionary<int, TokenedClient> tokenedClients = new Dictionary<int, TokenedClient> ();

		static TokenedClient ()
		{
			TokenLifetime = 600;
		}

		/// <summary>
		/// Gets or sets the max time inbetween refreshes for a token (in seconds.)
		/// </summary>
		public static int TokenLifetime
		{
			get;
			set;
		}

		protected static int GetToken ()
		{
			return DateTime.Now.Millisecond + 42;
		}

		public static TokenedClient GetTokenedClient (IConnection connection)
		{
			int token = 0;

			while (true)
			{
				token = GetToken ();
				lock (connectionLock)
				{
					if (!tokenedClients.ContainsKey (token))
						break;
				}
			}

			var authed = new TokenedClient (token, connection);
			lock (connectionLock)
			{
				connections.Add (connection, authed);
				tokenedClients.Add (authed.Token, authed);
			}

			return authed;
		}

		public static TokenedClient GetTokenedClient (int token)
		{
			TokenedClient client;
			lock (connectionLock)
			{
				tokenedClients.TryGetValue (token, out client);
			}

			if (!client.GetExpiredOrRefresh())
				return client;
			else
			{
				lock (connectionLock)
				{
					tokenedClients.Remove (token);
					connections.Remove (client.Connection);
				}

				client.Connection.Disconnect ();
			}

			return null;
		}
	}
}