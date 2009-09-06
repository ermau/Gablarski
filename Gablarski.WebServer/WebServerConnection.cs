using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gablarski.Messages;
using HttpServer.Sessions;

namespace Gablarski.WebServer
{
	public class WebServerConnection
		: IConnection
	{
		public WebServerConnection (IHttpSession session)
		{
			this.IsConnected = true;
			this.session = session;
		}

		#region Implementation of IConnection
		public event EventHandler<MessageReceivedEventArgs> MessageReceived;
		public event EventHandler<ConnectionEventArgs> Disconnected;

		/// <summary>
		/// Gets whether the connection is active.
		/// </summary>
		public bool IsConnected { get; private set; }

		/// <summary>
		/// Sends <paramref name="message"/> to the other end of the connection.
		/// </summary>
		/// <param name="message">The message to send.</param>
		/// <exception cref="System.ArgumentNullException"><paramref name="message"/> is <c>null</c>.</exception>
		public void Send (MessageBase message)
		{
			((List<MessageBase>)session["mqueue"]).Add (message);
		}

		/// <summary>
		/// Closes the connection.
		/// </summary>
		public void Disconnect()
		{
			this.IsConnected = false;
		}

		#endregion

		private readonly IHttpSession session;

		internal void Receive (MessageBase messageBase)
		{
			var received = this.MessageReceived;
			if (received != null)
				received (this, new MessageReceivedEventArgs (this, messageBase));
		}
	}
}
