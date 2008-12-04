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

		protected Message (UserConnection connection)
		{
			this.Connection = connection;
		}

		public TMessage MessageType
		{
			get;
			set;
		}

		public UserConnection Connection
		{
			get;
			private set;
		}

		public NetBuffer GetBuffer ()
		{
			this.buffer = this.Connection.CreateBuffer ();
			this.buffer.Write (FirstByte);
			this.buffer.WriteVariableUInt32 (this.MessageTypeCode);
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
			get { return (this.buffer ?? this.GetBuffer()); }
		}

		protected abstract uint MessageTypeCode
		{
			get;
		}
	}
}