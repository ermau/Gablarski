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
		public ServerNetworkConnectionProvider ()
		{
			this.Port = 6112;
		}

		public int Port
		{
			get;
			set;
		}

		#region IConnectionProvider Members

		public event EventHandler<ConnectionMadeEventArgs> ConnectionMade;

		public void StartListening ()
		{
			this.listening = true;
			this.listener = new TcpListener (this.Port);
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
		private TcpListener listener;

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