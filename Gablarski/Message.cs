using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lidgren.Network;

namespace Gablarski
{
	public abstract class Message<TMessage>
	{
		public const byte FirstByte = 42;

		protected Message (TMessage messageType)
		{
			this.MessageType = messageType;
		}

		protected Message (TMessage messageType, NetConnection connection)
			: this (messageType)
		{
			this.Connections = new List<NetConnection> { connection };
		}

		protected Message (TMessage messageType, IEnumerable<NetConnection> connections)
			: this (messageType)
		{
			this.Connections = connections;
		}

		public TMessage MessageType
		{
			get;
			private set;
		}

		public NetConnection Connection
		{
			get { return (this.Connections != null) ? this.Connections.FirstOrDefault() : null; }
		}

		public IEnumerable<NetConnection> Connections
		{
			get; private set;
		}

		public NetBuffer GetBuffer ()
		{
			return GetBuffer (0);
		}

		public NetBuffer GetBuffer (int capacity)
		{
			this.buffer = new NetBuffer (capacity + 8);

			this.buffer.Write (FirstByte);
			this.buffer.WriteVariableUInt32 (this.MessageTypeCode);

			if (this.SendAuthHash && this.Connection != null)
				this.buffer.WriteVariableInt32 ((int)this.Connection.Tag);

			return this.buffer;
		}

		public void Send (NetClient client, NetChannel channel)
		{
			client.SendMessage (this.Buffer, channel);
		}

		public void Send (NetServer server, NetChannel channel)
		{
			if (this.Connections.Any())
				server.SendMessage (this.Buffer, this.Connections, channel);
			else if (this.Connection != null)
				server.SendMessage (this.Buffer, this.Connection, channel);
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