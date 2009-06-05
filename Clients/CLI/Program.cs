using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gablarski.OpenAL;
using Gablarski.OpenAL.Providers;
using Gablarski.Server;
using Gablarski.Network;
using System.Diagnostics;
using Gablarski.Client;
using System.Threading;
using Gablarski.Messages;
using Gablarski;
using Gablarski.Media.Sources;
using NDesk.Options;
using Gablarski.OpenAL;

namespace Gablarski.Clients.CLI
{
	public class Program
	{
		public static GablarskiClient client;

		[STAThread]
		public static void Main (string[] args)
		{
			string host = String.Empty;
			int port = 6112;
			bool trace = false;

			string username = String.Empty;
			string password = String.Empty;
			string nickname = String.Empty;

			OptionSet options = new OptionSet
			{
				{ "h=|host=", h => host = h },
				{ "p:|port:", (int p) => port = p },
				{ "t|trace", v => trace = true },
				{ "u=|username=", u => username = u },
				{ "pw=|password=", p => password = p },
				{ "n=|nickname=", n => nickname = n }


			};

			foreach (string unused in options.Parse (args))
				Console.WriteLine ("Unrecognized option: " + unused);

			if (trace)
				Trace.Listeners.Add (new ConsoleTraceListener ());
			
			if (String.IsNullOrEmpty (nickname)
				|| String.IsNullOrEmpty (host))
			{
				options.WriteOptionDescriptions (Console.Out);
			}
		}

		private static GablarskiClient SetupClient (string host, int port, string nickname, string username, string password)
		{
			GablarskiClient sclient = new GablarskiClient (new ClientNetworkConnection ());
			sclient.Connect (host, port);
			sclient.Login (nickname, username, password);

			return sclient;
		}
	}
}