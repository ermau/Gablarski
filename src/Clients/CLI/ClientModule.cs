using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Gablarski.Client;

namespace Gablarski.Clients.CLI
{
	public class ClientModule
		: GClientModule
	{
		public ClientModule (GablarskiClient client)
			: base (client)
		{
			
		}

		public override bool Process (string part, TextWriter writer)
		{
			throw new NotImplementedException ();
		}
	}
}