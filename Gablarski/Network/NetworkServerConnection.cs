// Copyright (c) 2009, Eric Maupin
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
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Gablarski.Messages;

namespace Gablarski.Network
{
	public class NetworkServerConnection
		: IConnection
	{
		public NetworkServerConnection (uint nid, IPEndPoint endPoint, TcpClient tcp, IValueWriter unreliableWriter)
		{
			this.nid = nid;
			this.endPoint = endPoint;

			this.uwriter = unreliableWriter;

			this.tcp = tcp;

			this.stream = tcp.GetStream();
			this.reader = new StreamValueReader (stream);
			this.writer = new StreamValueWriter (stream);

			this.running = true;
			this.runnerThread = new Thread (this.Runner) { Name = "NetworkServerConnection Runner" };
			this.runnerThread.Start();
		}

		#region Implementation of IConnection

		/// <summary>
		/// Gets whether the connection is active.
		/// </summary>
		public bool IsConnected
		{
			get { return this.tcp.Connected; }
		}

		public event EventHandler<MessageReceivedEventArgs> MessageReceived;
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
			this.tcp.Close();

			this.runnerThread.Join();

			OnDisconnected();
		}

		public override string ToString ()
		{
			return "NetworkServerConnection " + this.endPoint;
		}

		#endregion

		private readonly IPEndPoint endPoint;

		private readonly IValueWriter uwriter;

		private readonly TcpClient tcp;
		private readonly NetworkStream stream;
		private readonly IValueReader reader;
		private readonly IValueWriter writer;

		private readonly Queue<MessageBase> sendQueue = new Queue<MessageBase>();

		private volatile bool running;
		private readonly Thread runnerThread;
		private volatile bool waiting;

		private readonly uint nid;

		internal void Receive (MessageBase message)
		{
			var received = this.MessageReceived;
			if (received != null)
				received (this, new MessageReceivedEventArgs (this, message));
		}

		private void Runner()
		{
			bool singleCore = (Environment.ProcessorCount == 1);
			const uint maxLoops = UInt32.MaxValue;
			uint loops = 0;

			IValueWriter urWriter = this.uwriter;
			IValueWriter rWriter = this.writer;

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
					try
					{
						IValueWriter iwriter = (!toSend.Reliable) ? urWriter : rWriter;
						iwriter.WriteByte (42);
						iwriter.WriteUInt16 (toSend.MessageTypeCode);

						toSend.WritePayload (iwriter);
						iwriter.Flush();
					}
					catch (Exception ex)
					{
						Trace.WriteLine ("[Server] Error writing, disconnecting: " + ex.Message);
						this.Disconnect();
						return;
					}
				}

				if (!this.waiting)
				{
					try
					{
						if (!this.tcp.Connected)
						{
							Trace.WriteLine ("[Server] Client disconnected.");
							this.Disconnect();
							return;
						}
						
						//if (this.stream.DataAvailable)
						//{
							this.waiting = true;
							byte[] mbuffer = new byte[1];

							this.stream.BeginRead (mbuffer, 0, 1, this.Received, mbuffer);
						//}
					}
					catch (Exception ex)
					{
						Trace.WriteLine ("[Server] Error starting read, disconnecting: " + ex.Message);
						this.Disconnect();
						return;
					}
				}

				if (!this.stream.DataAvailable)
					Thread.Sleep (1);
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

		private void Received (IAsyncResult ar)
		{
			try
			{
				if (this.stream.EndRead (ar) == 0)
					this.Disconnect();

				byte[] mbuffer = (ar.AsyncState as byte[]);

				if (mbuffer[0] != 0x2A)
					return;

				ushort type = this.reader.ReadUInt16();
				MessageBase msg;
				if (MessageBase.GetMessage (type, out msg))
				{
					msg.ReadPayload (this.reader);

					OnMessageReceived (new MessageReceivedEventArgs (this, msg));
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
				this.waiting = false;
			}
		}
	}
}