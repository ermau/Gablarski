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
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
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

			if (serverConfig.Network) {
				var keyFile = new FileInfo ("server.key");
				RSAAsymmetricKey key = null;
				if (keyFile.Exists) {
					try {
						using (var stream = File.OpenRead (keyFile.FullName)) {
							var reader = new StreamValueReader (stream);
							RSAParameters parameters = RSAParametersSerializer.Deserialize (reader);
							key = new RSAAsymmetricKey (parameters);
						}
					} catch (Exception ex) {
						Trace.TraceWarning ("Failed to read key: {0}", ex);
						keyFile.Delete();
					}
				}

				if (!keyFile.Exists) {
					var rsa = new RSACrypto();
					RSAParameters parameters = rsa.ExportKey (true);
					key = new RSAAsymmetricKey (parameters);
			
					using (var stream = File.OpenWrite (keyFile.FullName)) {
						var writer = new StreamValueWriter (stream);
						RSAParametersSerializer.Serialize (writer, parameters);
					}
				}

				server.AddConnectionProvider (new UdpConnectionProvider (serverConfig.Port, GablarskiProtocol.Instance, key), ExecutionMode.GlobalOrder);
			}

			foreach (IConnectionProvider provider in providers.ConnectionProviders)
				server.AddConnectionProvider (provider, ExecutionMode.GlobalOrder);

			server.Start();

			log.Info ("Server started");

			while (true)
				Console.ReadKey();
		}

		sealed class RSAParametersSerializer
			: ISerializer<RSAParameters>
		{
			public static readonly RSAParametersSerializer Instance = new RSAParametersSerializer();

			public static void Serialize (IValueWriter writer, RSAParameters element)
			{
				Instance.Serialize (null, writer, element);
			}

			public static RSAParameters Deserialize (IValueReader reader)
			{
				return Instance.Deserialize (null, reader);
			}

			public void Serialize (ISerializationContext context, IValueWriter writer, RSAParameters element)
			{
				writer.WriteBytes (element.D);
				writer.WriteBytes (element.DP);
				writer.WriteBytes (element.DQ);
				writer.WriteBytes (element.Exponent);
				writer.WriteBytes (element.InverseQ);
				writer.WriteBytes (element.Modulus);
				writer.WriteBytes (element.P);
				writer.WriteBytes (element.Q);
			}

			public RSAParameters Deserialize (ISerializationContext context, IValueReader reader)
			{
				return new RSAParameters {
					D = reader.ReadBytes(),
					DP = reader.ReadBytes(),
					DQ = reader.ReadBytes(),
					Exponent = reader.ReadBytes(),
					InverseQ = reader.ReadBytes(),
					Modulus = reader.ReadBytes(),
					P = reader.ReadBytes(),
					Q = reader.ReadBytes()
				};
			}
		}
	}
}