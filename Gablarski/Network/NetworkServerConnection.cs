using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gablarski.Messages; 
using System.Net.Sockets;
using System.Net;
using System.IO;

namespace Gablarski.Network
{
	public class NetworkServerConnection
		: IConnection
	{
		internal NetworkServerConnection (NetworkServerConnectionProvider provider, uint nid, TcpClient tcp, IPEndPoint endPoint, IValueWriter uwriter)
		{
			this.NetworkId = nid;
			this.Tcp = tcp;
			this.ReliableStream = tcp.GetStream ();
			this.provider = provider;
			this.EndPoint = endPoint;
			this.UnreliableWriter = uwriter;
			this.ReliableReader = new StreamValueReader (ReliableStream);
			this.ReliableWriter = new StreamValueWriter (ReliableStream);
		}

		public event EventHandler<MessageReceivedEventArgs> MessageReceived;
		public event EventHandler<ConnectionEventArgs> Disconnected;
		
		public bool IsConnected
		{
			get { return this.Tcp.Connected; }
		}

		public void Send (MessageBase message)
		{
			provider.EnqueueToSend (this, message);
		}

		public void Disconnect ()
		{
			this.Tcp.Close ();

			OnDisconnected ();
		}

		internal readonly uint NetworkId;
		internal readonly TcpClient Tcp;
		internal readonly Stream ReliableStream;
		internal readonly IPEndPoint EndPoint;
		internal readonly IValueWriter UnreliableWriter;
		internal readonly IValueReader ReliableReader;
		internal readonly IValueWriter ReliableWriter;

		internal void Receive (MessageBase message)
		{
			var received = this.MessageReceived;
			if (received != null)
				received (this, new MessageReceivedEventArgs (this, message));
		}

		private readonly NetworkServerConnectionProvider provider;		

		private void OnDisconnected ()
		{
			var dced = this.Disconnected;
			if (dced != null)
				dced (this, new ConnectionEventArgs (this));
		}
	}
}