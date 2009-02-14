using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gablarski.Messages;

namespace Gablarski.Network
{
	public class ClientNetworkConnection
		: IClientConnection
	{
		#region IClientConnection Members

		public void Connect (string host, int port)
		{
			throw new NotImplementedException ();
		}

		#endregion

		#region IConnection Members

		public event EventHandler<MessageReceivedEventArgs> MessageReceived;

		public void Send (MessageBase message)
		{
			throw new NotImplementedException ();
		}

		#endregion
	}
}