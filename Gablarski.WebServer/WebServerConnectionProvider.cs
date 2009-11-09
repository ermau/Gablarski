using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Gablarski.Messages;
using HttpServer;
using HttpServer.HttpModules;
using HttpServer.Sessions;

namespace Gablarski.WebServer
{
	public class WebServerConnectionProvider
		: IConnectionProvider
	{
		public int Port
		{
			get { return this.port; }
			set { this.port = value; }
		}

		#region Implementation of IConnectionProvider

		public event EventHandler<ConnectionlessMessageReceivedEventArgs> ConnectionlessMessageReceived;
		public event EventHandler<ConnectionEventArgs> ConnectionMade;

		/// <summary>
		/// Sends a connectionless <paramref name="message"/> to the <paramref name="endpoint"/>.
		/// </summary>
		/// <param name="message">The message to send.</param>
		/// <param name="endpoint">The endpoint to send the <paramref name="message"/> to.</param>
		/// <exception cref="ArgumentNullException"><paramref name="message"/> is <c>null</c>.</exception>
		/// <exception cref="ArgumentNullException"><paramref name="endpoint"/> is <c>null</c>.</exception>
		/// <exception cref="ArgumentException"><paramref name="message"/> is set as a reliable message.</exception>
		/// <seealso cref="IConnectionProvider.ConnectionlessMessageReceived"/>
		public void SendConnectionlessMessage(MessageBase message, EndPoint endpoint)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// Starts listening for connections and connectionless messages.
		/// </summary>
		public void StartListening()
		{
			var sstore = new MemorySessionStore();
			server = new HttpServer.HttpServer (sstore);

			ConnectionManager cmanager = new ConnectionManager();
			cmanager.ConnectionProvider = this;
			cmanager.Server = server;

			ControllerModule controller = new ControllerModule();
			controller.Add (new UserController (cmanager));
			controller.Add (new ChannelController (cmanager));

			server.Add (new FileResourceModule());
			server.Add (new LoginModule(cmanager));
			server.Add (new AdminModule(cmanager));
			server.Add (new QueryModule(cmanager));
			
			server.Start (IPAddress.Any, this.Port);
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
		private int port = 6113;

		internal protected void OnConnectionMade (ConnectionEventArgs e)
		{
			var made = this.ConnectionMade;
			if (made != null)
				made (this, e);
		}
	}
}