using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Gablarski.Messages;

namespace Gablarski.Network
{
	public class NetworkClientConnection
		: IClientConnection
	{
		#region Implementation of IConnection

		/// <summary>
		/// Gets whether the connection is active.
		/// </summary>
		public bool IsConnected
		{
			get { return this.tcp.Connected; }
		}

		/// <summary>
		/// Gets the identifying types for various data structures.
		/// </summary>
		public IdentifyingTypes IdentifyingTypes
		{
			get; set;
		}

		/// <summary>
		/// A message was received from the underlying transport.
		/// </summary>
		public event EventHandler<MessageReceivedEventArgs> MessageReceived;

		/// <summary>
		/// The underlying transport has been disconnected.
		/// </summary>
		public event EventHandler<ConnectionEventArgs> Disconnected;

		/// <summary>
		/// Sends <paramref name="message"/> to the other end of the connection.
		/// </summary>
		/// <param name="message">The message to send.</param>
		/// <exception cref="System.ArgumentNullException"><paramref name="message"/> is <c>null</c>.</exception>
		public void Send (MessageBase message)
		{
			lock (sendQueue)
			{
				sendQueue.Enqueue (message);
			}
		}

		/// <summary>
		/// Closes the connection.
		/// </summary>
		public void Disconnect()
		{
			this.running = false;

			if (this.tcp != null)
				this.tcp.Close();
			
			if (this.udp != null)
				this.udp.Close();

			if (this.runnerThread != null)
				this.runnerThread.Join();

			this.tcp = null;
			this.udp = null;
			
			this.rwriter = null;
			this.rreader = null;
			this.ureader = null;
			this.uwriter = null;
			this.rstream = null;
		}

		#endregion

		#region Implementation of IClientConnection

		/// <summary>
		/// The client has succesfully connected to the end point.
		/// </summary>
		public event EventHandler Connected;

		/// <summary>
		/// Connects to <paramref name="endpoint"/>.
		/// </summary>
		/// <param name="endpoint">The endpoint to connect to.</param>
		/// <exception cref="System.ArgumentNullException"><paramref name="endpoint"/> is <c>null</c>.</exception>
		public void Connect (IPEndPoint endpoint)
		{
			this.running = true;

			this.tcp = new TcpClient (new IPEndPoint (IPAddress.Any, 0));
			this.tcp.Connect (endpoint);
			this.rstream = this.tcp.GetStream();

			this.udp = new Socket (AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
			this.udp.Bind (new IPEndPoint (IPAddress.Any, 0));

			this.rwriter = new StreamValueWriter (this.rstream);
			this.rreader = new StreamValueReader (this.rstream);
			this.rwriter.WriteInt32 (((IPEndPoint)this.udp.LocalEndPoint).Port);
			this.nid = this.rreader.ReadUInt32();

			this.uwriter = new SocketValueWriter (this.udp, endpoint);
			
			this.runnerThread = new Thread (this.Runner) { Name = "NetworkClientConnection Runner" };
			this.runnerThread.Start();
		}

		#endregion

		private volatile bool running;
		private Thread runnerThread;

		private TcpClient tcp;
		private Stream rstream;
		private IValueWriter rwriter;
		private IValueReader rreader;
		private volatile bool rwaiting;

		private uint nid;
		private Socket udp;
		private IValueWriter uwriter;
		private IValueReader ureader;
		private volatile bool uwaiting;

		private readonly Queue<MessageBase> sendQueue = new Queue<MessageBase>();

		private void Runner()
		{
			const uint maxLoops = UInt32.MaxValue;
			uint loops = 0;
			bool singleCore = (Environment.ProcessorCount == 1);

			IValueWriter writeReliable = this.rwriter;
			IValueWriter writeUnreliable = this.uwriter;

			Queue<MessageBase> queue = this.sendQueue;

			while (this.running)
			{
				MessageBase toSend = null;
				lock (queue)
				{
					if (queue.Count > 0)
						toSend = queue.Dequeue();
				}

				if (toSend != null)
				{
					IValueWriter iwriter = (!toSend.Reliable) ? writeUnreliable : writeReliable;
					iwriter.WriteByte (0x2A);
					
					if (!toSend.Reliable)
						iwriter.WriteUInt32 (this.nid);

					iwriter.WriteUInt16 (toSend.MessageTypeCode);

					toSend.WritePayload (iwriter, this.IdentifyingTypes);
					iwriter.Flush();
				}

				if (!this.uwaiting && udp.Available > 3)
				{
					this.uwaiting = true;
					var ipEndpoint = new IPEndPoint (IPAddress.Any, 0);
					var tendpoint = (EndPoint)ipEndpoint;
					byte[] buffer = new byte[5120];
					udp.BeginReceiveFrom (buffer, 0, buffer.Length, SocketFlags.None, ref tendpoint, UnreliableReceive, buffer);
				}

				if (!this.rwaiting)
				{
					this.rwaiting = true;
					byte[] mbuffer = new byte[1];

					try
					{
						this.rstream.BeginRead (mbuffer, 0, 1, this.ReliableReceived, mbuffer);
					}
					catch (SocketException sex)
					{
						Trace.WriteLine ("[Server] Error starting read, disconnecting: " + sex.Message);
						this.Disconnect();
						return;
					}
				}

				if (singleCore || (++loops % 100) == 0)
					Thread.Sleep (1);
				else
					Thread.SpinWait (20);

				if (loops == maxLoops)
					loops = 0;
			}
		}

		private void OnMessageReceived (MessageReceivedEventArgs e)
		{
			var received = this.MessageReceived;
			if (received != null)
				received (this, e);
		}

		private void ReliableReceived (IAsyncResult ar)
		{
			try
			{
				this.rstream.EndRead (ar);
				byte[] mbuffer = (ar.AsyncState as byte[]);

				if (mbuffer[0] != 0x2A)
				{
					Trace.WriteLine ("[Client] Failed sanity check, disconnecting.");
					this.Disconnect();
					return;
				}

				ushort type = this.rreader.ReadUInt16();

				Func<MessageBase> messageCtor;
				MessageBase.MessageTypes.TryGetValue (type, out messageCtor);
				if (messageCtor != null)
				{
					var msg = messageCtor();
					msg.ReadPayload (this.rreader, this.IdentifyingTypes);

					OnMessageReceived (new MessageReceivedEventArgs (this, msg));
				}
				else
					this.Disconnect();
			}
			catch (Exception ex)
			{
				Trace.WriteLine ("[Client] Error reading payload, disconnecting: " + ex.Message);
				this.Disconnect();
				return;
			}
			finally
			{
				this.rwaiting = false;
			}
		}

		private void UnreliableReceive (IAsyncResult result)
		{
			try
			{
				var ipendpoint = new IPEndPoint (IPAddress.Any, 0);
				var endpoint = (EndPoint)ipendpoint;
				
				if (udp.EndReceiveFrom (result, ref endpoint) == 0)
					return;

				byte[] buffer = (byte[])result.AsyncState;

				if (buffer[0] != 0x2A)
					return;

				IValueReader reader = new ByteArrayValueReader (buffer, 1);
				ushort mtype = reader.ReadUInt16();

				Func<MessageBase> messageCtor;
				MessageBase.MessageTypes.TryGetValue (mtype, out messageCtor);
				if (messageCtor == null)
					return;
				else
				{
					var msg = messageCtor();
					msg.ReadPayload (reader, this.IdentifyingTypes);

					OnMessageReceived (new MessageReceivedEventArgs (this, msg));
				}
			}
			catch (SocketException sex)
			{
			}
			catch (ObjectDisposedException odex)
			{
			}
			finally
			{
				this.uwaiting = false;
			}
		}
	}
}