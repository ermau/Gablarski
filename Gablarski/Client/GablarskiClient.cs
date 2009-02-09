using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gablarski.Server;

namespace Gablarski.Client
{
	public class GablarskiClient
	{
		public IEnumerable<IConnection> Connections
		{
			get;
			private set;
		}
	}
}