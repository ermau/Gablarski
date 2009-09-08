﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using HttpServer;
using HttpServer.Sessions;

namespace Gablarski.WebServer
{
	public class WebServerConnectionProvider
		: IConnectionProvider
	{
		#region Implementation of IConnectionProvider

		public event EventHandler<MessageReceivedEventArgs> ConnectionlessMessageReceived;
		public event EventHandler<ConnectionEventArgs> ConnectionMade;

		/// <summary>
		/// Starts listening for connections and connectionless messages.
		/// </summary>
		public void StartListening()
		{
			var sstore = new MemorySessionStore();
			server = new HttpServer.HttpServer (sstore);

			ConnectionManager.SessionStore = sstore;
			ConnectionManager.ConnectionProvider = this;
			ConnectionManager.Server = server;

			server.Add (new FileResourceModule());
			server.Add (new LoginModule());
			server.Add (new AdminModule());
			server.Add (new QueryModule());
			
			server.Start (IPAddress.Any, 80);
		}

		/// <summary>
		/// Stops listening for connections and connectionless messages.
		/// </summary>
		public void StopListening()
		{
			server.Stop();
			server = null;
		}

		#endregion

		private HttpServer.HttpServer server;

		internal protected void OnConnectionMade (ConnectionEventArgs e)
		{
			var made = this.ConnectionMade;
			if (made != null)
				made (this, e);
		}
	}
}