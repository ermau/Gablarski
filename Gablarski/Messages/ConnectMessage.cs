using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gablarski.Messages
{
	public class ConnectMessage
		: ClientMessage
	{
		public ConnectMessage ()
			: base (ClientMessageType.Connect)
		{
		}

		public ConnectMessage (Version apiVersion)
			: this ()
		{
			this.ApiVersion = apiVersion;
		}

		public Version ApiVersion
		{
			get;
			private set;
		}

		public override void WritePayload (IValueWriter writer)
		{
			writer.WriteVersion (this.ApiVersion);
		}

		public override void ReadPayload (IValueReader reader)
		{
			this.ApiVersion = reader.ReadVersion ();
		}
	}
}