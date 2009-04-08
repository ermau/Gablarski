using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gablarski.Server;
using Gablarski.Network;
using System.Diagnostics;
using Gablarski.Client;
using System.Threading;

namespace GablarskiTester
{
	class Program
	{
		static void Main (string[] args)
		{
			Trace.Listeners.Add (new ConsoleTraceListener ());

			GablarskiServer server = new GablarskiServer (new GuestUserProvider ());
			server.AddConnectionProvider (new ServerNetworkConnectionProvider { Port = 6112 });

			GablarskiClient client = new GablarskiClient (new ClientNetworkConnection ());
			client.Connect ("localhost", 6112);
			client.Login ("Rogue Jedi");

			while (true)
			{
				Thread.Sleep (1);
			}
		}
	}
}