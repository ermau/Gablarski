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

using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Net;
using Gablarski.Barrel.Config;
using Gablarski.Server;
using Tempest;
using Tempest.Providers.Network;
using log4net;
using Cadenza;

namespace Gablarski.Barrel
{
	public class Barrel
	{
		public static void Main ()
		{
			Trace.Listeners.Add (new ConsoleTraceListener());

			log4net.Config.XmlConfigurator.Configure ();
			
			var serverConfig = (BarrelConfiguration)ConfigurationManager.GetSection ("barrel");
			if (serverConfig == null)
			{
				LogManager.GetLogger ("Barrel").Fatal ("Section 'barrel' not found in configuration.");
				return;
			}
			
			var log = LogManager.GetLogger (serverConfig.Name.Remove (" "));

			log.Info ("Checking configuration");
			if (!serverConfig.CheckConfiguration (log))
			{
				log.Fatal ("Errors found in configuration, shutting down.");
				return;
			}

			ServerProviders providers = serverConfig.GetProviders (log);
			if (providers == null)
			{
				log.Fatal ("Errors loading server configuration, shutting down.");
				return;
			}

			log.Info ("Setting up");

			GablarskiServer server = new GablarskiServer (new ServerSettings
			{
				Name = serverConfig.Name,
				Description = serverConfig.Description,
				ServerPassword = serverConfig.Password,
				ServerLogo = serverConfig.LogoURL
			}, providers.Users, providers.Permissions, providers.Channels);

			if (serverConfig.Network)
				server.AddConnectionProvider (new NetworkConnectionProvider (GablarskiProtocol.Instance, new Target (Target.AnyIP, serverConfig.Port), serverConfig.MaxConnections), ExecutionMode.GlobalOrder);

			foreach (IConnectionProvider provider in providers.ConnectionProviders)
				server.AddConnectionProvider (provider, ExecutionMode.GlobalOrder);

			server.Start();

			log.Info ("Server started");
		}
	}
}