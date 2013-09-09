// Copyright (c) 2011, Eric Maupin
// All rights reserved.
//
// Redistribution and use in source and binary forms, with
// or without modification, are permitted provided that
// the following conditions are met:
//
// - Redistributions of source code must retain the above 
//   copyright notice, this list of conditions and the
//   following disclaimer.
//
// - Redistributions in binary form must reproduce the above
//   copyright notice, this list of conditions and the
//   following disclaimer in the documentation and/or other
//   materials provided with the distribution.
//
// - Neither the name of Gablarski nor the names of its
//   contributors may be used to endorse or promote products
//   or services derived from this software without specific
//   prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS
// AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED
// WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
// WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR
// PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT
// HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT,
// INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
// (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE
// GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
// INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY,
// WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
// NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
// THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH
// DAMAGE.

using System;
using System.Collections.Generic;
using System.Reflection;
using Gablarski.Client;
using Tempest.Providers.Network;

namespace Gablarski.Clients.CLI
{
	public class Program
	{
		[STAThread]
		public static void Main (string[] args)
		{
			Console.WriteLine ("Gablarski CLI Client version {0}", Assembly.GetAssembly (typeof(Program)).GetName ().Version);
	
			GablarskiClient client = new GablarskiClient (new UdpClientConnection (GablarskiProtocol.Instance));
	
			List<CommandModule> modules = new List<CommandModule> 
			{
				new ChannelModule (client, Console.Out),
				new SourceModule (client, Console.Out),
				new ClientModule (client, Console.Out),
				new ProvidersModule (client, Console.Out)
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

						if (line == "st")
							line = "sources talk";
						else if (line == "et")
							line = "sources endtalk";

						foreach (var m in modules)
						{
							if (!m.Process (line))
								continue;
						}

						break;
				}
			}
		}
	}
}