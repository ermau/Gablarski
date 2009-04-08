using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gablarski.Server;
using Gablarski.Network;
using System.Diagnostics;

namespace GablarskiTester
{
	class Program
	{
		static void Main (string[] args)
		{
			Trace.Listeners.Add (new ConsoleTraceListener ());

			GablarskiServer server = new GablarskiServer (new GuestUserProvider ());
			server.AddConnectionProvider (new ServerNetworkConnectionProvider ());

		}
	}
}