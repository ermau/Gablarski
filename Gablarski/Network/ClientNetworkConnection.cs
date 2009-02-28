using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gablarski.Messages;
using System.Net.Sockets;
using System.IO;
using System.Threading;

namespace Gablarski.Network
{
	public class ClientNetworkConnection
		: IClientConnection
	{
		#region IClientConnection Members

		public void Connect (string host, int port)
		{
			tcp.Connect (host, port);
			this.rstream = tcp.GetStream ();
			this.rwriter = new StreamValueWriter (this.rstream);
			this.rreader = new StreamValueReader (this.rstream);

			udp.Connect (host, port);
		}

		public void Disconnect ()
		{
			this.running = false;
			tcp.Close ();

			if (this.runnerThread != null)
				this.runnerThread.Join ();
		}

		#endregion

		#region IConnection Members

		public event EventHandler<MessageReceivedEventArgs> MessageReceived;

		public void Send (MessageBase message)
		{
			lock (queuel)
			{
				mqueue.Enqueue (message);
			}
		}

		#endregion

		private Thread runnerThread;
		private volatile bool running = true;

		private object queuel = new object ();
		private Queue<MessageBase> mqueue = new Queue<MessageBase> ();

		private TcpClient tcp = new TcpClient ();
		private UdpClient udp = new UdpClient ();

		private NetworkStream rstream;
		private IValueWriter rwriter;
		private IValueReader rreader;

		private IValueWriter uwriter;
		private IValueReader ureader;

		private bool waiting;

		protected virtual void OnMessageReceived (MessageReceivedEventArgs e)
		{
			var received = this.MessageReceived;
			if (received != null)
				received (this, e);
		}

		private IValueWriter GetWriter (MessageBase message)
		{
			return (message.Reliable) ? this.rwriter : this.uwriter;
		}

		private void Received (IAsyncResult read)
		{
			this.rstream.EndRead (read);
			byte[] mbuffer = (byte[])read.AsyncState;

			if (mbuffer[0] != 0x2A)
			{
				this.Disconnect ();
				return;
			}

			MessageBase msg = MessageBase.MessageTypes[this.rreader.ReadUInt16 ()] ();
			msg.ReadPayload (this.rreader);

			this.OnMessageReceived (new MessageReceivedEventArgs (this, msg));
		}

		private void Runner ()
		{
			while (this.running)
			{
				if (!this.waiting)
				{
					this.waiting = true;
					byte[] mbuffer = new byte[3];
					this.rstream.BeginRead (mbuffer, 0, 3, this.Received, mbuffer);
				}

				while (mqueue.Count > 0)
				{
					MessageBase message;
					lock (queuel)
					{
						message = this.mqueue.Dequeue ();
					}
					message.WritePayload (GetWriter (message));
				}
			}
		}
	}
}