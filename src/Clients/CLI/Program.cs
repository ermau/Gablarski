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
			Console.WriteLine ("Gablarski CLI Client version {0}", Assembly.GetAssembly (typeof (Program)).GetName().Version);

			GablarskiClient client = new GablarskiClient (new NetworkClientConnection());

			List<CommandModule> modules = new List<CommandModule>
			{
				new ClientModule (client, Console.Out),
				new ChannelModule (client, Console.Out),

				//new ServerModule (),
			};

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

					case "":
						break;

					default:
						if (line.StartsWith ("connect"))
							line = line.Replace ("connect", "client connect");
							
						if (line.StartsWith ("disconnect"))
							line = line.Replace ("disconnect", "client disconnect");

						if (line.StartsWith ("join"))
							line = line.Replace ("join", "client join");

						foreach (var m in modules)
						{
							if (m.Process (line))
								break;
						}

						break;
				}
			}


		}
	}
}