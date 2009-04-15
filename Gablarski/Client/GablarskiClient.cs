using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gablarski.Server;
using Gablarski.Network;
using Gablarski.Messages;
using System.Diagnostics;
using System.Reflection;
using Gablarski.Media.Sources;

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
		public event EventHandler<ReceivedSourceEventArgs> ReceivedSource;
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
			Login (nickname, null, null);
		}

		public void Login (string nickname, string username, string password)
		{
			CheckToken ();

			this.connection.Send (new LoginMessage
			{
				Token = token,
				Nickname = nickname,
				Username = username,
				Password = password
			});
		}

		public void RequestSource (Type mediaSourceType, byte channels)
		{
			this.connection.Send (new RequestSourceMessage (this.token, mediaSourceType, channels));
		}
		#endregion

		#region Internals
		protected readonly Dictionary<ServerMessageType, Action<MessageReceivedEventArgs>> Handlers;
		protected readonly IClientConnection connection;
		protected int token;

		private object sourceLock = new object ();
		private List<IMediaSource> sources = new List<IMediaSource> ();

		protected void CheckToken ()
		{
			if (this.token == 0)
				throw new InvalidOperationException ("Must receive token before attempting any action.");
		}

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
			var sourceMessage = (SourceResultMessage)e.Message;

			OnReceivedSourceResult (new ReceivedSourceEventArgs (sourceMessage.GetSource (), sourceMessage.SourceResult));
		}

		protected virtual void OnReceivedSourceResult (ReceivedSourceEventArgs e)
		{
			var received = this.ReceivedSource;
			if (received != null)
				received (this, e);
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

	public class ReceivedSourceEventArgs
		: EventArgs
	{
		public ReceivedSourceEventArgs (IMediaSource source, SourceResult result)
		{
			this.Result = result;
			this.Source = source;
		}

		public SourceResult Result
		{
			get;
			set;
		}

		public IMediaSource Source
		{
			get;
			set;
		}
	}
}