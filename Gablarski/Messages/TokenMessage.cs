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

		public TokenMessage (IValueReader reader)
			: base (ServerMessageType.Token, reader)
		{
		}

		public int Token
		{
			get;
			set;
		}

		protected override void ReadPayload (IValueReader reader)
		{
			this.Token = reader.ReadInt32 ();
		}

		protected override void WritePayload (IValueWriter writer)
		{
			writer.WriteInt32 (this.Token);
		}
	}
}