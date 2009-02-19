using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gablarski.Messages
{
	public class RequestTokenMessage
		: ClientMessage
	{
		public RequestTokenMessage (IEndPoint endpoint)
			: base (ClientMessages.RequestToken, endpoint)
		{
		}

		public int AuthHash
		{
			get;
			set;
		}

		protected override void WritePayload (IValueWriter writer)
		{
			writer.WriteInt32 (this.AuthHash);
		}

		protected override void ReadPayload (IValueReader reader)
		{
			this.AuthHash = reader.ReadInt32 ();
		}
	}
}