using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Gablarski.Audio;
using Gablarski.Client;
using Gablarski.Network;
using Gablarski.Server;
using Mono.Options;
using System.Linq;
using Cadenza;

namespace Gablarski.Clients.CLI
{
	public class Program
	{
		[STAThread]
		public static void Main (string[] args)
		{
			string host = null;
			int port;
			string nickname;

			OptionSet options = new OptionSet
			{
				{ "version", v => Console.WriteLine ("Gablarski CLI Client version {0}", Assembly.GetAssembly (typeof (Program)).GetName().Version) },

				{ "h=|host=",		h => host = h },
				{ "p:|port:",		(int p) => port = p },
				//{ "u=|username=",	u => username = u },
				//{ "pw=|password=",	p => password = p },
				{ "n=|nickname=",	n => nickname = n },
				
				//{ "voicesource",	"Requests a voice source upon login.",							v => voiceSource = (v != null) },
				//{ "defaultaudio",	"Selects the default input and output audio.",					v => defaultAudio = (v != null) },

				//{ "v|verbose",		"Turns on tracing, detailed debug invormation",					v => trace = (v != null) },
				//{ "V|veryverbose",	"Turns on very detailed debug information",						v => veryverbose = (v != null) },
				//{ "tracer=",		"Adds a tracer to the list (supply assembly qualified name.)",	tracers.Add },

				//{ "s|server",		"Starts a local Server.",										s => startServer = true },
				//{ "serverlogo=",	"Specifies a server logo URL.",									v => serverLogo = v },
				//{ "conprovider=",	"Adds a connection provider to the Server.",					connectionProviders.Add	},
			};

			options.Parse (args);

			GablarskiClient client = null;
			if (!host.IsNullOrWhitespace())
				client = new GablarskiClient (new NetworkClientConnection());

			List<CommandModule> modules = new List<CommandModule>();
			
			if (client != null)
			{
				modules.Add (new ClientModule (client));
				modules.Add (new ChannelModule (client));
			}

			bool exit = false;
			while (!exit)
			{
				string line = Console.ReadLine();
				switch (line.Trim().ToLower())
				{
					case "exit":
					case "quit":
						exit = true;
						break;

					default:
						break;
				}
			}


		}
	}
}