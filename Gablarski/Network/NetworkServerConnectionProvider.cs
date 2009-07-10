using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Gablarski.Messages;

namespace Gablarski.Network
{
	public class NetworkServerConnectionProvider
		: IConnectionProvider
	{
		public int Port
		{
			get { return this.port; }
			set { this.port = value; }
		}

		#region Implementation of IConnectionProvider

		/// <summary>
		/// A connectionless message was received.
		/// </summary>
		public event EventHandler<MessageReceivedEventArgs> ConnectionlessMessageReceived;

		/// <summary>
		/// A connection was made.
		/// </summary>
		public event EventHandler<ConnectionEventArgs> ConnectionMade;

		public IdentifyingTypes IdentifyingTypes
		{
			get; set;
		}

		/// <summary>
		/// Starts listening for connections and connectionless messages.
		/// </summary>
		public void StartListening()
		{
			this.listening = true;
			udp = new Socket (AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
			udp.Bind (new IPEndPoint (IPAddress.Any, port));

			tcpListener = new TcpListener (IPAddress.Any, port);
			tcpListener.Start();

			this.listenerThread = new Thread (this.Listener) { Name = "Network Provider Listener" };
			this.listenerThread.Start();
		}

		/// <summary>
		/// Stops listening for connections and connectionless messages.
		/// </summary>
		public void StopListening()
		{
			this.listening = false;
			udp.Close();
			tcpListener.Stop();

			if (this.listenerThread != null)
			{
				this.listenerThread.Join();
				this.listenerThread = null;
			}

			udp = null;
			tcpListener = null;
		}

		#endregion

		private Thread listenerThread;
		private volatile bool waiting;
		private volatile bool listening;
		private TcpListener tcpListener;
		private Socket udp;
		private int port = 6112;
		private volatile bool accepting;

		private readonly Dictionary<IPEndPoint, NetworkServerConnection> connections = new Dictionary<IPEndPoint, NetworkServerConnection>();

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

				NetworkServerConnection connection;
				lock (connections)
				{
					connections.TryGetValue (ipendpoint, out connection);
				}

				byte[] buffer = (byte[])result.AsyncState;

				if (buffer[0] == 0x2A)
					return;

				IValueReader reader = new ByteArrayValueReader (buffer);
				ushort mtype = reader.ReadUInt16();

				Func<MessageBase> messageCtor;
				MessageBase.MessageTypes.TryGetValue (mtype, out messageCtor);
				if (messageCtor == null && connection != null)
					connection.Disconnect();
				else if (messageCtor != null)
				{
					var msg = messageCtor();
					msg.ReadPayload (reader, this.IdentifyingTypes);

					if (connection == null)
						OnConnectionlessMessageReceived (new MessageReceivedEventArgs (null, msg));
					else
						connection.Receive (msg);
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
				this.waiting = false;
			}
		}

		protected void OnConnectionlessMessageReceived (MessageReceivedEventArgs e)
		{
			var received = this.ConnectionlessMessageReceived;
			if (received != null)
				received (this, e);
		}

		private void AcceptConnection (object result)
		{
			try
			{
				var listener = (result as TcpListener);
				#if DEBUG
				if (listener == null)
					throw new ArgumentException ("result");
				#endif

				TcpClient client = listener.AcceptTcpClient();
				var endpoint = (IPEndPoint)client.Client.RemoteEndPoint;
				var connection = new NetworkServerConnection (endpoint, client, new SocketValueWriter (this.udp));
				lock (connections)
				{
					connections.Add (endpoint, connection);
				}

				OnConnectionMade (new ConnectionEventArgs (connection));
			}
			catch (SocketException sex)
			{
				Trace.WriteLine ("[Server] Failed to accept connection: " + sex.Message);
			}
			finally
			{
				this.accepting = false;
			}
		}

		protected void OnConnectionMade (ConnectionEventArgs e)
		{
			var connection = this.ConnectionMade;
			if (connection != null)
				connection (this, e);
		}

		private void Listener()
		{
			const uint maxLoops = UInt32.MaxValue;
			uint loops = 0;
			bool singleCore = (Environment.ProcessorCount == 1);

			while (this.listening)
			{
				if (!this.waiting && udp.Available > 3)
				{
					this.waiting = true;
					var ipEndpoint = new IPEndPoint (IPAddress.Any, 0);
					var tendpoint = (EndPoint)ipEndpoint;
					byte[] buffer = new byte[5120];
					udp.BeginReceiveMessageFrom (buffer, 0, 5120, SocketFlags.None, ref tendpoint, UnreliableReceive, buffer);
				}

				if (!this.accepting && tcpListener.Pending())
				{
					this.accepting = true;
					ThreadPool.QueueUserWorkItem (AcceptConnection, tcpListener);
				}

				if (singleCore || (++loops % 100) == 0)
					Thread.Sleep (1);
				else
					Thread.SpinWait (20);

				if (loops == maxLoops)
					loops = 0;
			}
		}
	}
}