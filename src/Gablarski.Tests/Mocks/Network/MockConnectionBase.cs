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
using System.IO;
using System.Linq;
using System.Threading;
using Gablarski.Messages;
using NUnit.Framework;

namespace Gablarski.Tests
{
	public abstract class MockConnectionBase
		: IConnection
	{
		protected MockConnectionBase ()
		{
			this.readStream = new MemoryStream (this.buffer, false);
			this.writeStream = new MemoryStream (this.buffer, true);

			this.reader = new StreamValueReader (this.readStream);
			this.writer = new StreamValueWriter (this.writeStream);
		}

		public bool IsAsync
		{
			get { return true; }
		}

		public void Receive (MessageBase message)
		{
			lock (this.buffer)
			{
				writer.WriteByte (42);
				writer.WriteUInt16 (message.MessageTypeCode);
				message.WritePayload (this.writer);
				Interlocked.Increment (ref this.waiting);
			}

			var received = this.MessageReceived;
			if (received != null)
				received (this, new MessageReceivedEventArgs (this, message));
		}

		public MessageBase DequeueMessage ()
		{
			return DequeueMessage (true);
		}

		public MessageBase DequeueMessage (bool wait)
		{
			UInt16 tick = 0;
			while (wait && this.waiting == 0 && tick++ < (UInt16.MaxValue - 1))
				Thread.Sleep (1);

			if (this.waiting == 0)
				return null;

			if (tick == UInt16.MaxValue)
				Assert.Fail ("[" + Name + "] Message never arrived.");

			MessageBase msg;
			lock (this.buffer)
			{
				byte sanity = this.reader.ReadByte();
				if (sanity != 0x2A)
					Assert.Fail ("[" + Name + "] Header does not begin with 42.");

				ushort type = this.reader.ReadUInt16();
				if (!MessageBase.GetMessage (type, out msg))
					Assert.Fail ("[" + Name + "] Type " + type + " does not exist.");

				msg.ReadPayload (this.reader);

				Interlocked.Decrement (ref this.waiting);
			}

			return msg;
		}

		public IEnumerable<ReceivedMessage> Tick()
		{
			while (this.waiting > 0)
			{
				yield return new ReceivedMessage (DequeueMessage (false));
			}
		}

		public T DequeueAndAssertMessage<T> ()
			where T : MessageBase
		{
			var message = this.DequeueMessage ();
			Assert.IsInstanceOf<T> (message);

			return (T)message;
		}

		public void AssertNoMessage()
		{
			DateTime start = DateTime.Now;
			while (DateTime.Now.Subtract (start).TotalSeconds < .5)
				Thread.Sleep (1);

			if (this.waiting > 0)
				Assert.Fail (DequeueMessage().GetType().Name + " was waiting");
		}

		#region IConnection Members
		public event EventHandler<MessageReceivedEventArgs> MessageReceived;
		public event EventHandler<ConnectionEventArgs> Disconnected;

		public bool IsConnected
		{
			get { return this.connected; }
		}

		public abstract void Send (MessageBase message);

		public virtual void Disconnect ()
		{
			this.connected = false;
			var dced = this.Disconnected;
			if (dced != null)
				dced (this, new ConnectionEventArgs (this));
		}

		#endregion

		private bool connected = true;

		private readonly StreamValueWriter writer;
		private readonly StreamValueReader reader;
		private readonly byte[] buffer = new byte[20480];
		private readonly MemoryStream readStream;
		private readonly MemoryStream writeStream;
		private int waiting = 0;

		protected abstract string Name
		{
			get;
		}
	}
}