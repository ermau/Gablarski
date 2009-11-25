using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Gablarski.Server;

namespace Gablarski.Clients.CLI
{
	public class ServerModule
		: GServerModule
	{
		public ServerModule(GablarskiServer server)
			: base (server)
		{
		}

		#region Overrides of CommandModule

		public override bool Process (string part, TextWriter writer)
		{
			throw new NotImplementedException();
		}

		#endregion
	}
}
