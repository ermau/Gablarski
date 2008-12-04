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

		public abstract void Encode (NetBuffer buffer);

		protected void EncodeHeader (NetBuffer buffer)
		{
			buffer.Write (FirstByte);
			buffer.WriteVariableInt32 (this.Connection.AuthHash);
		}
	}
}