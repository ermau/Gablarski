using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Threading;

namespace Gablarski.Network
{
	public class ServerNetworkConnectionProvider
		: IConnectionProvider
	{
		#region IConnectionProvider Members

		public event EventHandler<ConnectionMadeEventArgs> ConnectionMade;

		public void StartListening ()
		{
			this.listening = true;
			listener.Start ();
			(this.listenChecker = new Thread (this.ListenChecker)
			{
				IsBackground = true,
				Name = "ServerNetworkConnectionProvider Listener"
			}).Start ();
		}

		public void StopListening ()
		{
			listener.Stop ();
		}

		#endregion

		private volatile bool listening;
		private Thread listenChecker;
		private TcpListener listener = new TcpListener (6112);

		private void ListenChecker ()
		{
			while (this.listening)
			{
				TcpClient client = listener.AcceptTcpClient ();
				this.OnConnectionMade (new ConnectionMadeEventArgs (new ServerNetworkConnection (client)));
			}
		}

		protected virtual void OnConnectionMade (ConnectionMadeEventArgs e)
		{
			var connection = this.ConnectionMade;
			if (connection != null)
				connection (this, e);
		}
	}
}