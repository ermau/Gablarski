using System;

namespace Gablarski.Messages
{
	public class PunchThroughReceivedMessage
		: ServerMessage
	{
		public PunchThroughReceivedMessage ()
			: base (ServerMessageType.PunchThroughReceived)
		{
		}
		
		public override bool Reliable
		{
			get { return false; }
		}
		
		public override void WritePayload (IValueWriter writer)
		{
		}

		public override void ReadPayload (IValueReader reader)
		{
		}
	}
}