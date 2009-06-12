using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gablarski.Messages
{
	public enum ConnectionRejectedReason
	{
		Unknown = 0,
		IncompatibleVersion = 1
	}

	public class ConnectionRejectedMessage
		: ServerMessage
	{
		public ConnectionRejectedMessage ()
			: base (ServerMessageType.ConnectionRejected)
		{
		}

		public ConnectionRejectedMessage (ConnectionRejectedReason reason)
			: this()
		{
			this.Reason = reason;
		}

		public ConnectionRejectedReason Reason
		{
			get;
			private set;
		}

		public override void WritePayload (IValueWriter writer)
		{
			writer.WriteByte ((byte)this.Reason);
		}

		public override void ReadPayload (IValueReader reader)
		{
			this.Reason = (ConnectionRejectedReason)reader.ReadByte ();
		}
	}
}