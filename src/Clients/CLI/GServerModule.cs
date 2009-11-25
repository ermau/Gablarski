using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Gablarski.Server;

namespace Gablarski.Clients.CLI
{
	public abstract class GServerModule
		: CommandModule
	{
		private readonly GablarskiServer server;

		protected GServerModule (GablarskiServer server)
		{
			this.server = server;
		}

		protected GablarskiServer Server
		{
			get { return this.server; }
		}
	}
}