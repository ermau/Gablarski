using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gablarski.Messages
{
	public enum ConnectionRejectedReason
	{
		Unknown = 0,
		
		/// <summary>
		/// Server rejected the client as incompatible.
		/// </summary>
		IncompatibleVersion = 1,

		/// <summary>
		/// The client could not connect to the server.
		/// </summary>
		CouldNotConnect = 2,
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

		public override void WritePayload (IValueWriter writer, IdentifyingTypes idTypes)
		{
			writer.WriteByte ((byte)this.Reason);
		}

		public override void ReadPayload (IValueReader reader, IdentifyingTypes idTypes)
		{
			this.Reason = (ConnectionRejectedReason)reader.ReadByte ();
		}
	}
}