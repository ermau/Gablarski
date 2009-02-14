using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Rocks;

namespace Gablarski.Messages
{
	public class LoginMessage
		: ClientMessage
	{
		public LoginMessage (AuthedClient client)
			: base (ClientMessages.Login, client)
		{
		}

		protected override void WritePayload (IValueWriter writer)
		{
			throw new NotImplementedException ();
		}
	}
}