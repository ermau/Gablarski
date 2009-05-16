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

		public event EventHandler Disconnected;

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