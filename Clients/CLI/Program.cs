using System;
using System.Diagnostics;
using Gablarski.Client;
using Gablarski.Network;
using Mono.Options;

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
				{ "h=|host=",		h => host = h },
				{ "p:|port:",		(int p) => port = p },
				{ "t|trace",		v => trace = true },
				{ "u=|username=",	u => username = u },
				{ "pw=|password=",	p => password = p },
				{ "n=|nickname=",	n => nickname = n }
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
			sclient.CurrentUser.Login (nickname, username, password);

			return sclient;
		}
	}
}