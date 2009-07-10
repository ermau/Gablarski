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
			get { throw new NotImplementedException(); }
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
			throw new NotImplementedException();
		}

		/// <summary>
		/// Closes the connection.
		/// </summary>
		public void Disconnect()
		{
			this.tcp.Close();
			this.udp.Close();

			this.tcp = null;
			this.udp = null;
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
			this.tcp = new TcpClient (new IPEndPoint (IPAddress.Any, 0));
			this.tcp.Connect (endpoint);
			this.rstream = this.tcp.GetStream();

			this.rwriter = new StreamValueWriter (this.rstream);
			this.rreader = new StreamValueReader (this.rstream);

			this.udp = new Socket (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			this.udp.Connect (endpoint.Address, endpoint.Port);

			this.uwriter = new SocketValueWriter (this.udp);
		}

		#endregion

		private volatile bool running;

		private TcpClient tcp;
		private Stream rstream;
		private IValueWriter rwriter;
		private IValueReader rreader;
		private volatile bool rwaiting;

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
					iwriter.WriteByte (42);
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
					udp.BeginReceiveMessageFrom (buffer, 0, 5120, SocketFlags.None, ref tendpoint, UnreliableReceive, buffer);
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
					Trace.WriteLine ("[Server] Failed sanity check, disconnecting.");

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
			}
			catch (Exception ex)
			{
				Trace.WriteLine ("[Server] Error reading payload, disconnecting: " + ex.Message);
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
				SocketFlags flags = SocketFlags.None;
				IPPacketInformation packetInfo;
				if (udp.EndReceiveMessageFrom (result, ref flags, ref endpoint, out packetInfo) == 0)
					return;

				byte[] buffer = (byte[])result.AsyncState;

				if (buffer[0] == 0x2A)
					return;

				IValueReader reader = new ByteArrayValueReader (buffer);
				ushort mtype = reader.ReadUInt16();

				Func<MessageBase> messageCtor;
				MessageBase.MessageTypes.TryGetValue (mtype, out messageCtor);
				if (messageCtor == null)
					this.Disconnect();
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