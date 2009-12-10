using System;

namespace Gablarski.Messages
{
	public enum PunchThroughStatus
		: byte
	{
		Punch = 0,
		Bleeding = 1
	}

	public class PunchThroughMessage
		: ClientMessage
	{
		public PunchThroughMessage ()
			: base (ClientMessageType.PunchThrough)
		{
		}
		
		public PunchThroughMessage (PunchThroughStatus status)
			: this()
		{
			Status = status;
		}
		
		public PunchThroughStatus Status
		{
			get;
			set;
		}
		
		public override bool Reliable
		{
			get	{ return false; }
		}

		public override void WritePayload (IValueWriter writer)
		{
			writer.WriteByte ((byte)Status);
		}
		
		public override void ReadPayload (IValueReader reader)
		{
			Status = (PunchThroughStatus)reader.ReadByte();
		}
	}
}