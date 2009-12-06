using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Gablarski.Clients.CLI
{
	public abstract class CommandModule
	{
		public abstract bool Process (string line);
	}
}