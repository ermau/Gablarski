using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gablarski.Server;
using Gablarski.Network;
using Gablarski.Messages;
using System.Diagnostics;

namespace Gablarski.Client
{
	public class GablarskiClient
	{
		public GablarskiClient (IClientConnection connection)
		{
			this.connection = connection;

			this.Handlers = new Dictionary<ServerMessageType, Action<MessageReceivedEventArgs>>
			{
				{ ServerMessageType.Token, OnTokenReceived },
				{ ServerMessageType.LoginResult, OnLoginResult },
				{ ServerMessageType.SourceResult, OnSourceReceived }
			};
		}

		public void Connect (string host, int port)
		{
			connection.MessageReceived += OnMessageReceived;
			connection.Connect (host, port);
		}

		public void Login (string nickname)
		{
			this.connection.Send (new LoginMessage
			{
				Nickname = nickname
			});
		}

		public void Login (string nickname, string username, string password)
		{
			this.connection.Send (new LoginMessage
			{
				Nickname = nickname,
				Username = username,
				Password = password
			});
		}

		public void RequestSource (MediaType type, byte channels)
		{
			this.connection.Send (new RequestSourceMessage (this.token, type, channels));
		}

		private readonly Dictionary<ServerMessageType, Action<MessageReceivedEventArgs>> Handlers;

		private readonly IClientConnection connection;
		private int token;

		protected virtual void OnMessageReceived (object sender, MessageReceivedEventArgs e)
		{
			var msg = (e.Message as ServerMessage);
			if (msg == null)
			{
				connection.Disconnect ();
				return;
			}

			Trace.WriteLine ("[Client] Message Received: " + msg.MessageType.ToString ());

			this.Handlers[msg.MessageType] (e);
		}

		protected virtual void OnTokenReceived (MessageReceivedEventArgs e)
		{
			this.token = ((TokenMessage)e.Message).Token;
		}

		protected virtual void OnLoginResult (MessageReceivedEventArgs e)
		{

		}

		protected virtual void OnSourceReceived (MessageReceivedEventArgs e)
		{

		}
	}
}