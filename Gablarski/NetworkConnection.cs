using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gablarski
{
	public class NetworkMessageReceiver
		: IConnection
	{
		public NetworkMessageReceiver (int port)
		{
		}

		#region IMessageReceiver<TMessage> Members

		public event EventHandler<MessageReceivedEventArgs> MessageReceived = delegate { };

		public void StartListening ()
		{
			throw new NotImplementedException ();
		}

		public void StopListening ()
		{
			throw new NotImplementedException ();
		}

		public void Send (MessageBase message)
		{
			throw new NotImplementedException ();
		}

		#endregion

		protected void OnMessageReceived (MessageReceivedEventArgs e)
		{
			MessageReceived (this, e);
		}
	}
}