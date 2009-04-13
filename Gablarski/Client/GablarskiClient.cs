using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gablarski.Server;
using Gablarski.Network;
using Gablarski.Messages;
using System.Diagnostics;
using System.Reflection;

namespace Gablarski.Client
{
	public class GablarskiClient
	{
		public static readonly Version ClientVersion = Assembly.GetAssembly (typeof (GablarskiClient)).GetName ().Version;

		protected GablarskiClient ()
		{
		}

		public GablarskiClient (IClientConnection connection)
		{
			this.connection = connection;

			this.Handlers = new Dictionary<ServerMessageType, Action<MessageReceivedEventArgs>>
			{
				{ ServerMessageType.TokenResult, OnTokenReceived },
				{ ServerMessageType.LoginResult, OnLoginResult },
				{ ServerMessageType.SourceResult, OnSourceReceived }
			};
		}

		#region Events
		public event EventHandler<ReceivedTokenEventArgs> ReceivedToken;
		public event EventHandler<ReceivedLoginEventArgs> ReceivedLogin;
		#endregion

		#region Public Methods
		public void Connect (string host, int port)
		{
			connection.MessageReceived += OnMessageReceived;
			connection.Connect (host, port);
			connection.Send (new RequestTokenMessage (ClientVersion));
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
		#endregion

		#region Internals
		protected readonly Dictionary<ServerMessageType, Action<MessageReceivedEventArgs>> Handlers;
		protected readonly IClientConnection connection;
		protected int token;

		protected virtual void OnMessageReceived (object sender, MessageReceivedEventArgs e)
		{
			var msg = (e.Message as ServerMessage);
			if (msg == null)
			{
				connection.Disconnect ();
				return;
			}

			this.Handlers[msg.MessageType] (e);
		}

		protected void OnTokenReceived (MessageReceivedEventArgs e)
		{
			var msg = (TokenResultMessage)e.Message;
			this.token = msg.Token;

			OnReceivedTokenResult (new ReceivedTokenEventArgs (msg.Result));
		}

		protected virtual void OnReceivedTokenResult (ReceivedTokenEventArgs e)
		{
			var result = this.ReceivedToken;
			if (result != null)
				result (this, e);
		}

		protected void OnLoginResult (MessageReceivedEventArgs e)
		{
			OnReceivedLoginResult (new ReceivedLoginEventArgs (((LoginResultMessage)e.Message).Result));
		}

		protected virtual void OnReceivedLoginResult (ReceivedLoginEventArgs e)
		{
			var result = this.ReceivedLogin;
			if (result != null)
				result (this, e);
		}

		protected void OnSourceReceived (MessageReceivedEventArgs e)
		{

		}
		#endregion
	}

	public class ReceivedLoginEventArgs
		: EventArgs
	{
		public ReceivedLoginEventArgs (LoginResult result)
		{
			this.Result = result;
		}

		public LoginResult Result
		{
			get;
			private set;
		}
	}

	public class ReceivedTokenEventArgs
		: EventArgs
	{
		public ReceivedTokenEventArgs (TokenResult result)
		{
			this.Result = result;
		}

		public TokenResult Result
		{
			get;
			private set;
		}
	}
}