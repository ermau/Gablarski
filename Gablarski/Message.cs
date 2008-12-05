using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lidgren.Network;

namespace Gablarski
{
	public abstract class Message<TMessage>
	{
		public const byte FirstByte = (byte)42;

		protected Message (TMessage messageType)
		{
			this.MessageType = messageType;
		}

		protected Message (TMessage messageType, UserConnection connection)
			: this (messageType)
		{
			this.Connection = connection;
		}

		public TMessage MessageType
		{
			get;
			private set;
		}

		public UserConnection Connection
		{
			get;
			private set;
		}

		public NetBuffer GetBuffer ()
		{
			if (this.Connection != null)
				this.buffer = this.Connection.CreateBuffer ();
			else
				this.buffer = new NetBuffer (4);

			this.buffer.Write (FirstByte);
			this.buffer.WriteVariableUInt32 (this.MessageTypeCode);

			if (this.SendAuthHash && this.Connection != null)
				this.buffer.WriteVariableInt32 (this.Connection.AuthHash);

			return this.buffer;
		}

		public void Send (NetClient client, NetChannel channel)
		{
			client.SendMessage (this.Buffer, channel);
		}

		public void Send (NetServer server, NetConnection recipient, NetChannel channel)
		{
			server.SendMessage (this.Buffer, recipient, channel);
		}

		public void Send (NetServer server, IList<NetConnection> recipients, NetChannel channel)
		{
			server.SendMessage (this.Buffer, recipients, channel);
		}

		private NetBuffer buffer;

		protected NetBuffer Buffer
		{
			get { return (this.buffer ?? this.GetBuffer ()); }
		}

		protected abstract uint MessageTypeCode
		{
			get;
		}

		protected abstract bool SendAuthHash
		{
			get;
		}
	}
}