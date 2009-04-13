using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gablarski.Messages
{
	public class RequestTokenMessage
		: ClientMessage
	{
		public RequestTokenMessage ()
			: base (ClientMessageType.RequestToken)
		{
		}

		public int AuthHash
		{
			get;
			set;
		}

		public override void WritePayload (IValueWriter writer)
		{
			writer.WriteInt32 (this.AuthHash);
		}

		public override void ReadPayload (IValueReader reader)
		{
			this.AuthHash = reader.ReadInt32 ();
		}
	}
}