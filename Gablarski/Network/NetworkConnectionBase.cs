using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gablarski.Messages;
using System.Net.Sockets;
using System.Threading;

namespace Gablarski.Network
{
	public abstract class NetworkConnectionBase
		: IConnection
	{
		protected NetworkConnectionBase ()
		{
			this.tcp = new TcpClient ();
			this.udp = new UdpClient ();
		}

		protected NetworkConnectionBase (TcpClient client)
		{
			this.tcp = client;
			this.udp = new UdpClient ();
		}

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

		public void Disconnect ()
		{
			this.running = false;
			tcp.Close ();

			if (this.runnerThread != null)
				this.runnerThread.Join ();
		}

		protected Thread runnerThread;
		protected volatile bool running = true;

		protected object queuel = new object ();
		protected Queue<MessageBase> mqueue = new Queue<MessageBase> ();

		protected readonly TcpClient tcp;
		protected readonly UdpClient udp;

		protected NetworkStream rstream;
		protected IValueWriter rwriter;
		protected IValueReader rreader;

		protected IValueWriter uwriter;
		protected IValueReader ureader;

		protected bool waiting;

		protected void StartListener ()
		{
			(this.runnerThread = new Thread (Runner)
			{
				IsBackground = true,
				Name = "NetworkConnectionBase Listener"
			}).Start();
		}

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
			MessageBase msg = null;

			this.rstream.EndRead (read);
			byte[] mbuffer = (byte[])read.AsyncState;

			try
			{
				if (mbuffer[0] != 0x2A)
				{
					this.Disconnect ();
					return;
				}

				msg = MessageBase.MessageTypes[this.rreader.ReadUInt16 ()] ();
				msg.ReadPayload (this.rreader);
			}
			catch (Exception e)
			{
#if DEBUG
				throw e;
#else
				Trace.WriteLine ("Error reading payload, disconnecting.");
				this.Disconnect ();
				return;
#endif
			}
			finally
			{
				this.waiting = false;
			}

			this.OnMessageReceived (new MessageReceivedEventArgs (this, msg));
		}

		protected void Runner ()
		{
			while (this.running)
			{
				if (!this.waiting)
				{
					this.waiting = true;
					byte[] mbuffer = new byte[1];
					this.rstream.BeginRead (mbuffer, 0, 1, this.Received, mbuffer);
				}

				while (mqueue.Count > 0)
				{
					MessageBase message;
					lock (queuel)
					{
						message = this.mqueue.Dequeue ();
					}

					var writer = GetWriter (message);
					
					writer.WriteByte (0x2A);
					writer.WriteUInt16 (message.MessageTypeCode);

					message.WritePayload (writer);
				}

				Thread.Sleep (1);
			}
		}
	}
}