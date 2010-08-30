// Copyright (c) 2010, Eric Maupin
// All rights reserved.

// Redistribution and use in source and binary forms, with
// or without modification, are permitted provided that
// the following conditions are met:

// - Redistributions of source code must retain the above 
//   copyright notice, this list of conditions and the
//   following disclaimer.

// - Redistributions in binary form must reproduce the above
//   copyright notice, this list of conditions and the
//   following disclaimer in the documentation and/or other
//   materials provided with the distribution.

// - Neither the name of Gablarski nor the names of its
//   contributors may be used to endorse or promote products
//   derived from this software without specific prior
//   written permission.

// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS
// AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED
// WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
// WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR
// PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT
// HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT,
// INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
// (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE
// GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
// INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY,
// WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
// NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
// THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH
// DAMAGE.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Gablarski.Messages;
using Cadenza;
using Gablarski.Server;

namespace Gablarski.Network
{
	public class NetworkServerConnectionProvider
		: IConnectionProvider
	{
		public NetworkServerConnectionProvider()
		{
			log = log4net.LogManager.GetLogger ("Network");
			debugLogging = log.IsDebugEnabled;
		}

		public event EventHandler<ConnectionlessMessageReceivedEventArgs> ConnectionlessMessageReceived;
		public event EventHandler<ConnectionEventArgs> ConnectionMade;

		public int Port
		{
			get { return this.port; }
			set { this.port = value; }
		}

		public void SendConnectionlessMessage (MessageBase message, EndPoint endpoint)
		{
			lock (outgoingConnectionless)
			{
				outgoingConnectionless.Enqueue (new ConnectionlessMessageReceivedEventArgs (this, message, endpoint));
			}

			this.outgoingWait.Set();
		}

		public void StartListening (IServerContext context)
		{
			this.listening = true;

			var localEp = new IPEndPoint (IPAddress.Any, port);

			udp = new Socket (AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
			udp.EnableBroadcast = true;
			udp.Bind (localEp);

			this.clWriter = new SocketValueWriter (udp);

			tcpListener = new TcpListener (localEp);
			tcpListener.Start ();
			tcpListener.BeginAcceptSocket (AcceptConnection, null);

			(incomingThread = new Thread (Incoming)
			{
				Name = "Incoming",
				IsBackground = true
			}).Start ();

			(outgoingThread = new Thread (Outgoing)
			{
				Name = "Outgoing",
				IsBackground = true
			}).Start ();
		}

		public void StopListening ()
		{
			this.listening = false;

			tcpListener.Stop ();
			udp.Close ();

			if (this.outgoingThread != null)
			{
				if (outgoingWait != null)
					outgoingWait.Set ();

				this.outgoingThread.Join ();
				this.outgoingThread = null;
			}

			if (this.incomingThread != null)
			{
				this.incomingThread.Join();
				this.incomingThread = null;
			}

			udp = null;
			tcpListener = null;
		}

		internal void EnqueueToSend (IConnection connection, MessageBase message)
		{
			var sc = connection as NetworkServerConnection;

			lock (outgoing)
			{
				outgoing.Enqueue (new KeyValuePair<NetworkServerConnection, MessageBase> (sc, message));
			}

			outgoingWait.Set ();
		}

		internal void Disconnect (NetworkServerConnection connection)
		{
			log.DebugFormat ("Disconnecting connection {0} with NID: {1}", connection.EndPoint, connection.NetworkId);

			lock (connections)
			{
				connections.Remove (connection.NetworkId);
			}
		}

		internal void DisconnectAsync (NetworkServerConnection connection)
		{
			lock (disconnecting)
			{
				disconnecting.Add (connection);
			}

			outgoingWait.Set();
		}

		private volatile bool listening;
		private int port = 42912;
		private Socket udp;
		private TcpListener tcpListener;
		private readonly Dictionary<uint, NetworkServerConnection> connections = new Dictionary<uint, NetworkServerConnection> (20);
		private readonly log4net.ILog log;
		private readonly bool debugLogging;

		private readonly AutoResetEvent outgoingWait = new AutoResetEvent (false);
		private readonly Queue<KeyValuePair<NetworkServerConnection, MessageBase>> outgoing = new Queue<KeyValuePair<NetworkServerConnection, MessageBase>> (100);		
		private readonly HashSet<NetworkServerConnection> disconnecting = new HashSet<NetworkServerConnection>();
		
		private SocketValueWriter clWriter;
		private readonly Queue<ConnectionlessMessageReceivedEventArgs> outgoingConnectionless = new Queue<ConnectionlessMessageReceivedEventArgs>();

		private Thread outgoingThread;
		private Thread incomingThread;

		private void Incoming ()
		{
			var cs = this.connections;
			var tl = this.tcpListener;

			byte[] buffer = new byte[5120];
			var reader = new ByteArrayValueReader (buffer);

			var ipEndPoint = new IPEndPoint (IPAddress.Any, 0);
			var tendpoint = (EndPoint)ipEndPoint;

			while (this.listening)
			{
				try
				{
					if (udp == null || udp.ReceiveFrom (buffer, ref tendpoint) == 0)
					{
						log.Warn ("UDP ReceiveFrom returned nothing");
						return;
					}

					if (buffer[0] != 0x2A)
					{
						if (buffer[0] == 24)
							udp.SendTo (new byte[] { 24, 24 }, tendpoint);

						continue;
					}

					reader.Position = 1;

					uint nid = reader.ReadUInt32 ();

					NetworkServerConnection connection;
					lock (connections)
					{
						connections.TryGetValue (nid, out connection);
					}

					ushort mtype = reader.ReadUInt16 ();

					MessageBase msg;
					if (!MessageBase.GetMessage (mtype, out msg))
					{
						log.WarnFormat ("Message type {0} not found from {1}", mtype, connection);
						return;
					}
					
					msg.ReadPayload (reader);

					if (connection == null)
					{
						if (msg.AcceptedConnectionless)
						{
							if (this.debugLogging)
								this.log.DebugFormat ("Connectionless message received {0} from {1}", mtype, tendpoint);

							OnConnectionlessMessageReceived (new ConnectionlessMessageReceivedEventArgs (this, msg, tendpoint));
						}
						else
						{
							this.log.WarnFormat ("NID {2} ({0}) attempted to send message {1} connectionlessly", tendpoint, mtype, nid);
						}
					}
					else
					{
						if (msg.MessageTypeCode == (ushort)ClientMessageType.PunchThrough)
						{
							var punch = (PunchThroughMessage)msg;
							if (punch.Status == PunchThroughStatus.Punch)
								connection.Send (new PunchThroughReceivedMessage ());
							else if (punch.Status == PunchThroughStatus.Bleeding)
								connection.bleeding = true;
						}
						else
						{
							if (this.debugLogging)
								this.log.DebugFormat ("Message received {0}", mtype);

							connection.Receive (msg);
						}
					}
				}
				catch (SocketException)
				{
				}
				catch (ObjectDisposedException)
				{
				}
			}
		}

		private void OnConnectionlessMessageReceived (ConnectionlessMessageReceivedEventArgs e)
		{
			var received = this.ConnectionlessMessageReceived;
			if (received != null)
				received (this, e);
		}

		private void Outgoing ()
		{
			var o = this.outgoing;
			var ocl = this.outgoingConnectionless;
			var ow = this.outgoingWait;

			while (this.listening)
			{
				if (o.Count == 0 || ocl.Count == 0)
					ow.WaitOne ();

				while (o.Count > 0)
				{
					KeyValuePair<NetworkServerConnection, MessageBase> m;
					lock (o)
					{
						m = o.Dequeue ();
					}

					Send (m.Key, m.Value);
				}

				while (ocl.Count > 0)
				{
					ConnectionlessMessageReceivedEventArgs e;
					lock (ocl)
					{
						e = ocl.Dequeue();
					}

					SendConnectionless (e.EndPoint, e.Message);
				}

				if (disconnecting.Count > 0)
				{
					if (o.Count > 0)
						continue;

					lock (disconnecting)
					{
						foreach (NetworkServerConnection c in disconnecting)
							c.Disconnect();
					}
				}
			}
		}

		private void AcceptConnection (IAsyncResult result)
		{
			try
			{
				if (tcpListener == null)
					return;

				TcpClient client = tcpListener.EndAcceptTcpClient (result);
				client.NoDelay = true;

				var stream = client.GetStream ();
				var tendpoint = (IPEndPoint)client.Client.RemoteEndPoint;

				uint nid = 0;
				NetworkServerConnection connection;
				lock (connections)
				{
					while (connections.ContainsKey (++nid)) ;

					log.InfoFormat ("Accepted TCP Connection from {0}. Given NID: {1}", tendpoint, nid);

					connection = new NetworkServerConnection (this, nid, client, tendpoint, new SocketValueWriter (this.udp, tendpoint));
					connections.Add (nid, connection);
				}

				stream.Write (BitConverter.GetBytes (nid), 0, sizeof (uint));

				OnConnectionMade (new ConnectionEventArgs (connection));

				byte[] buffer = new byte[1];
				client.GetStream ().BeginRead (buffer, 0, 1, ReliableReceive, new Tuple<NetworkServerConnection, byte[]> (connection, buffer));

				tcpListener.BeginAcceptTcpClient (AcceptConnection, null);
			}
			catch (SocketException sex)
			{
				log.Info ("Failed to accept connection", sex);
			}
			catch (IOException ioex)
			{
				log.Info ("Failed to accept connection", ioex);
			}
			catch (ObjectDisposedException odex)
			{
				log.Info ("Failed to accept connection", odex);
			}
		}

		private void ReliableReceive (IAsyncResult result)
		{
			NetworkServerConnection connection = null;
			MessageBase msg = null;

			try
			{
				var state = (Tuple<NetworkServerConnection, byte[]>)result.AsyncState;
				connection = state.Item1;
				var stream = connection.ReliableStream;

				if (stream.EndRead (result) == 0)
				{
					connection.Disconnect ();
					return;
				}

				if (state.Item2[0] == 0x2A)
				{
					ushort type = connection.ReliableReader.ReadUInt16 ();
					if (MessageBase.GetMessage (type, out msg))
					{
						if (this.debugLogging)
							this.log.DebugFormat ("Message type {0} received from {1}", msg.MessageTypeCode, connection.EndPoint);

						msg.ReadPayload (connection.ReliableReader);

						connection.Receive (msg);
					}
				}

				if (connection.IsConnected)
					stream.BeginRead (state.Item2, 0, 1, ReliableReceive, state);
				else
					connection.Disconnect ();
			}
			catch (IOException)
			{
				if (connection != null)
					connection.Disconnect();
			}
			catch (SocketException)
			{
				if (connection != null)
					connection.Disconnect();
			}
			catch (Exception ex)
			{
				if (connection != null)
				{
					if (msg != null)
						log.Warn ("Error receiving message " + msg.MessageTypeCode + " from NID " + connection.NetworkId + " (" + connection.EndPoint + ")", ex);
					else
						log.Warn ("Error receiving from NID " + connection.NetworkId + " (" + connection.EndPoint + ")", ex);
				
					connection.Disconnect();
				}
				else
				{
					log.Warn ("Error receiving", ex);
				}
			}
		}

		private void SendConnectionless (EndPoint endpoint, MessageBase message)
		{
			try
			{
				clWriter.EndPoint = endpoint;

				clWriter.WriteByte (42);
				clWriter.WriteUInt16 (message.MessageTypeCode);

				message.WritePayload (clWriter);
				clWriter.Flush ();
			}
			catch (IOException) { }
			catch (SocketException) { }
			catch (Exception ex)
			{
				log.Warn ("Error sending to " + endpoint, ex);
			}
		}

		private void Send (NetworkServerConnection connection, MessageBase message)
		{
			try
			{
				IValueWriter iwriter;
				if (!message.Reliable && (connection.bleeding || message.MessageTypeCode == (ushort)ServerMessageType.PunchThroughReceived))
					iwriter = connection.UnreliableWriter;
				else
					iwriter = connection.ReliableWriter;

				iwriter.WriteByte (42);
				iwriter.WriteUInt16 (message.MessageTypeCode);

				if (message.Encrypted)
				{
					MemoryStream buffer;
					if (message.MessageSize != 0)
						buffer = new MemoryStream (message.MessageSize);
					else
						buffer = new MemoryStream();

					message.WritePayload (new StreamValueWriter (buffer));

					byte[] encrypted = connection.Encryption.Encrypt (buffer.GetBuffer());
					iwriter.WriteBytes (encrypted);
				}
				else
					message.WritePayload (iwriter);

				iwriter.Flush ();
			}
			catch (IOException)
			{
				connection.Disconnect();
			}
			catch (SocketException)
			{
				connection.Disconnect();
			}
			catch (Exception ex)
			{
				log.Warn ("Error sending " + message.MessageTypeCode + " to NID " + connection.NetworkId + "(" + connection.EndPoint + ")", ex);
				connection.Disconnect ();
			}
		}

		private void OnConnectionMade (ConnectionEventArgs e)
		{
			var connection = this.ConnectionMade;
			if (connection != null)
				connection (this, e);
		}
	}
}