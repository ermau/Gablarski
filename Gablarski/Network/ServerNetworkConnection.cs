using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gablarski.Network
{
	public class ServerNetworkConnection
		: IConnection
	{

		#region IConnection Members

		public event EventHandler<MessageReceivedEventArgs> MessageReceived;

		public void Send (Gablarski.Messages.MessageBase message)
		{
			throw new NotImplementedException ();
		}

		public void Disconnect ()
		{
			throw new NotImplementedException ();
		}

		#endregion
	}
}