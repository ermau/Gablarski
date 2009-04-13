using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gablarski.Server;
using Gablarski.Network;
using System.Diagnostics;
using Gablarski.Client;
using System.Threading;
using Gablarski.Messages;

namespace GablarskiTester
{
	class Program
	{
		static void Main (string[] args)
		{
			Trace.Listeners.Add (new ConsoleTraceListener ());

			Console.Write ("Username: ");
			string username = Console.ReadLine ();

			GablarskiServer server = new GablarskiServer (new GuestUserProvider ());
			server.AddConnectionProvider (new ServerNetworkConnectionProvider { Port = 6112 });

			GablarskiClient client = new GablarskiClient (new ClientNetworkConnection ());
			client.ReceivedToken += client_ReceivedTokenResult;
			client.ReceivedLogin += client_ReceivedLogin;
			client.Connect ("localhost", 6112);
			client.Login (username);

			while (true)
			{
				Thread.Sleep (1);
			}
		}

		static void client_ReceivedLogin (object sender, ReceivedLoginEventArgs e)
		{
			Console.WriteLine ("Login result: " + e.Result);
		}

		static void client_ReceivedTokenResult (object sender, ReceivedTokenEventArgs e)
		{
			Console.WriteLine ("Token result: " + e.Result);
		}
	}
}