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

		public RequestTokenMessage (Version clientVersion)
			: base (ClientMessageType.RequestToken)
		{
			this.ClientVersion = clientVersion;
		}

		public Version ClientVersion
		{
			get;
			set;
		}

		public override void WritePayload (IValueWriter writer)
		{
			writer.WriteVersion (this.ClientVersion);
		}

		public override void ReadPayload (IValueReader reader)
		{
			this.ClientVersion = reader.ReadVersion ();
		}
	}
}