using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gablarski.Messages
{
	public class TokenMessage
		: ServerMessage
	{
		public TokenMessage (int token)
			: base (ServerMessageType.Token)
		{
		}

		public int Token
		{
			get;
			set;
		}

		public override void ReadPayload (IValueReader reader)
		{
			this.Token = reader.ReadInt32 ();
		}

		public override void WritePayload (IValueWriter writer)
		{
			writer.WriteInt32 (this.Token);
		}
	}
}