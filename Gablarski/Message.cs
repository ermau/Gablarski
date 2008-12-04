using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gablarski
{
	public abstract class Message<TMessage>
		where TMessage : Enum
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

		public abstract byte[] ToByteArray ();
	}
}