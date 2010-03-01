// Copyright (c) 2010, Eric Maupin
// All rights reserved.
//
// Redistribution and use in source and binary forms, with
// or without modification, are permitted provided that
// the following conditions are met:
//
// - Redistributions of source code must retain the above 
//   copyright notice, this list of conditions and the
//   following disclaimer.
//
// - Redistributions in binary form must reproduce the above
//   copyright notice, this list of conditions and the
//   following disclaimer in the documentation and/or other
//   materials provided with the distribution.
//
// - Neither the name of Gablarski nor the names of its
//   contributors may be used to endorse or promote products
//   or services derived from this software without specific
//   prior written permission.
//
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
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Gablarski.Messages;

namespace Gablarski.Network
{
	public class NetworkClientConnection
		: IClientConnection
	{
		public NetworkClientConnection()
		{
			this.log = log4net.LogManager.GetLogger ("CNetwork");
			debugLogging = this.log.IsDebugEnabled;
		}
	
		#region Implementation of IConnection

		/// <summary>
		/// Gets whether the connection is active.
		/// </summary>
		public bool IsConnected
		{
			get { return (this.tcp == null) ? false : this.tcp.Connected; }
		}

		public bool IsAsync
		{
			get { return true; }
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
			if (debugLogging)
				log.DebugFormat ("Enqueuing {1} message {0} for send", message.MessageTypeCode, (message.Reliable) ? "reliable" : "unreliable");

			lock (sendQueue)
			{
				sendQueue.Enqueue (message);
			}

			this.sendWait.Set ();
		}

		public IEnumerable<ReceivedMessage> Tick()
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// Closes the connection.
		/// </summary>
		public void Disconnect()
		{
			log.Info ("Disconnecting");
			
			this.running = false;

			try
			{
				if (this.tcp != null)
				{
					this.rstream.Close();
					this.tcp.Close();
				}

				if (this.udp != null)
					this.udp.Close();
			}
			catch (Exception ex)
			{
				log.Warn ("Exception during disconnection.", ex);
			}

			lock (this.sendQueue)
			{
				this.sendWait.Set ();
				this.sendQueue.Clear();
			}

			if (this.runnerThread != null)
				this.runnerThread.Join();

			this.runnerThread = null;
			this.tcp = null;
			this.udp = null;
			
			this.rwriter = null;
			this.rreader = null;
			this.ureader = null;
			this.uwriter = null;
			this.rstream = null;

			OnDisconnected();
		}

		#endregion

		/// <summary>
		/// Whether to output detailed tracing.
		/// </summary>
		public bool VerboseTracing
		{
			get; set;
		}

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
			log.InfoFormat ("Connecting to {0}", endpoint);
			
			this.running = true;

			this.tcp = new TcpClient (new IPEndPoint (IPAddress.Any, 0));
			this.tcp.Connect (endpoint);
			this.rstream = this.tcp.GetStream();

			log.DebugFormat ("TCP Local Endpoint: {0}", this.tcp.Client.LocalEndPoint);

			this.udp = new Socket (AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
			this.udp.Bind ((IPEndPoint)this.tcp.Client.LocalEndPoint);

			pinger = new Timer (Ping, endpoint, 45000, 45000);

			log.DebugFormat ("UDP Local Endpoint: {0}", this.udp.LocalEndPoint);

			this.rwriter = new StreamValueWriter (this.rstream);
			this.rreader = new StreamValueReader (this.rstream);

			//this.rwriter.WriteInt32 (((IPEndPoint)this.udp.LocalEndPoint).Port);
			this.nid = this.rreader.ReadUInt32();

			this.uwriter = new SocketValueWriter (this.udp, endpoint);
			
			byte[] rbuffer = new byte[1];
			this.rstream.BeginRead (rbuffer, 0, 1, ReliableReceived, rbuffer);

			var ipendpoint = new IPEndPoint (IPAddress.Any, 0);
			var tendpoint = (EndPoint)ipendpoint;
			byte[] urbuffer = new byte[5120];
			this.udp.BeginReceiveFrom (urbuffer, 0, urbuffer.Length, SocketFlags.None, ref tendpoint, UnreliableReceive, urbuffer);
			
			Send (new PunchThroughMessage (PunchThroughStatus.Punch));

			this.runnerThread = new Thread (this.Runner) { Name = "NetworkClientConnection Runner" };
			this.runnerThread.Start();
		}

		private void Ping (object endpoint)
		{
			this.udp.SendTo (new byte[] { 24, 24 }, (IPEndPoint)endpoint);
		}

		#endregion

		private readonly bool debugLogging;

		private volatile bool running;
		private Thread runnerThread;

		private readonly log4net.ILog log;
		private TcpClient tcp;
		private NetworkStream rstream;
		private IValueWriter rwriter;
		private IValueReader rreader;
		private volatile bool rwaiting;

		private uint nid;
		private Socket udp;
		private IValueWriter uwriter;
		private IValueReader ureader;
		private volatile bool uwaiting;

		private readonly AutoResetEvent sendWait = new AutoResetEvent (false);
		private readonly Queue<MessageBase> sendQueue = new Queue<MessageBase>();
		private Timer pinger;

		private void Runner()
		{
			IValueWriter writeReliable = this.rwriter;
			IValueWriter writeUnreliable = this.uwriter;

			AutoResetEvent wait = this.sendWait;
			Queue<MessageBase> queue = this.sendQueue;

			while (this.running)
			{
				MessageBase toSend = null;

				while (queue.Count > 0)
				{
					lock (queue)
					{
						toSend = queue.Dequeue ();
					}

					if (debugLogging)
						log.DebugFormat ("Dequeued {1} message {0} for send", toSend.MessageTypeCode, (toSend.Reliable) ? "reliable" : "unreliable");

					IValueWriter iwriter = (!toSend.Reliable) ? writeUnreliable : writeReliable;
					iwriter.WriteByte (0x2A);

					if (!toSend.Reliable)
						iwriter.WriteUInt32 (this.nid);

					iwriter.WriteUInt16 (toSend.MessageTypeCode);

					toSend.WritePayload (iwriter);
					iwriter.Flush ();
				}

				if (queue.Count == 0)
					wait.WaitOne ();
			}
		}

		private void OnMessageReceived (MessageReceivedEventArgs e)
		{
			var received = this.MessageReceived;
			if (received != null)
				received (this, e);
		}

		private void OnDisconnected()
		{
			var dced = this.Disconnected;
			if (dced != null)
				dced (this, new ConnectionEventArgs (this));
		}

		private void ReliableReceived (IAsyncResult ar)
		{
			try
			{
				this.rstream.EndRead (ar);
				byte[] mbuffer = (ar.AsyncState as byte[]);

				if (mbuffer[0] != 0x2A)
				{
					log.Error ("Failed reliable sanity check, disconnecting.");
					Disconnect();
					return;
				}

				ushort type = this.rreader.ReadUInt16();

				MessageBase msg;
				if (MessageBase.GetMessage (type, out msg))
				{
					msg.ReadPayload (this.rreader);
					
					if (debugLogging)
						log.DebugFormat ("Received message {0} from server", msg.MessageTypeCode);

					OnMessageReceived (new MessageReceivedEventArgs (this, msg));

					if (rstream != null && running)
						rstream.BeginRead (mbuffer, 0, 1, ReliableReceived, mbuffer);
				}
				else
					this.Disconnect();
			}
			catch (Exception ex)
			{
				log.Error ("Error reading payload, disconnecting", ex);
				Disconnect();
				return;
			}
			finally
			{
				this.rwaiting = false;
			}
		}

		private void UnreliableReceive (IAsyncResult result)
		{
			var ipendpoint = new IPEndPoint (IPAddress.Any, 0);
			var endpoint = (EndPoint)ipendpoint;

			byte[] buffer = (byte[])result.AsyncState;

			if (udp == null)
				return;

			try
			{
				if (udp.EndReceiveFrom (result, ref endpoint) == 0)
				{
					log.Debug ("UDP EndReceiveFrom returned nothing.");
					return;
				}

				if (buffer[0] != 0x2A)
				{
					log.Warn ("Unreliable message failed sanity check.");
					return;
				}

				IValueReader reader = new ByteArrayValueReader (buffer, 1);
				ushort mtype = reader.ReadUInt16();

				MessageBase msg;
				if (!MessageBase.GetMessage (mtype, out msg))
				{
					log.WarnFormat ("Message type {0} not found from {1}", mtype, endpoint);
					return;
				}
				else
				{
					if (debugLogging)
						log.DebugFormat ("Received message {0} from {1}", msg.MessageTypeCode, endpoint);
				
					msg.ReadPayload (reader);
					
					if (msg.MessageTypeCode == (ushort)ServerMessageType.PunchThroughReceived)
						Send (new PunchThroughMessage (PunchThroughStatus.Bleeding));
					else
						OnMessageReceived (new MessageReceivedEventArgs (this, msg));
				}
			}
			catch (SocketException sex)
			{
				log.Warn ("Error during unreliable receive", sex);
			}
			catch (ObjectDisposedException odex)
			{
				log.Warn ("Error during unreliable receive", odex);
			}
			finally
			{
				this.uwaiting = false;

				if (udp != null && running)
					udp.BeginReceiveFrom (buffer, 0, buffer.Length, SocketFlags.None, ref endpoint, UnreliableReceive, buffer);
			}
		}
	}
}