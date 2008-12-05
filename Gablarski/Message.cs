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
			this.UserConnections = new List<UserConnection> { connection };
		}

		protected Message (TMessage messageType, IEnumerable<UserConnection> connections)
			: this (messageType)
		{
			this.UserConnections = connections;
		}

		public TMessage MessageType
		{
			get;
			private set;
		}

		public UserConnection UserConnection
		{
			get { return (this.UserConnections != null) ? this.UserConnections.FirstOrDefault() : null; }
		}

		public IEnumerable<UserConnection> UserConnections
		{
			get; private set;
		}

		public NetBuffer GetBuffer ()
		{
			if (this.UserConnection != null)
				this.buffer = this.UserConnection.CreateBuffer ();
			else
				this.buffer = new NetBuffer (4);

			this.buffer.Write (FirstByte);
			this.buffer.WriteVariableUInt32 (this.MessageTypeCode);

			if (this.SendAuthHash && this.UserConnection != null)
				this.buffer.WriteVariableInt32 (this.UserConnection.AuthHash);

			return this.buffer;
		}

		public void Send (NetClient client, NetChannel channel)
		{
			client.SendMessage (this.Buffer, channel);
		}

		public void Send (NetServer server, NetChannel channel)
		{
			if (this.UserConnection == null)
				throw new InvalidOperationException ("No connection set");

			if (this.UserConnections.Any())
				server.SendMessage (this.Buffer, this.UserConnections.Select (uc => uc.Connection).ToList(), channel);
			else
				server.SendMessage (this.Buffer, this.UserConnection.Connection, channel);
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