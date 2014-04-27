using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using Gablarski.Messages;

namespace Gablarski.Server.Telnet
{
	public class TelnetConnection
		: IConnection
	{
		internal TelnetConnection (TcpClient client)
		{
			this.client = client;
		}

		#region IConnection Members

		public event EventHandler<MessageReceivedEventArgs> MessageReceived;
		public event EventHandler<ConnectionEventArgs> Disconnected;

		public bool IsConnected
		{
			get { return this.client.Connected; }
		}

		public IdentifyingTypes IdentifyingTypes
		{
			get; set;
		}

		public void Send (MessageBase message)
		{
			throw new NotImplementedException ();
		}

		public void Disconnect ()
		{
			this.client.Close();
		}

		#endregion

		private readonly TcpClient client;
	}
}