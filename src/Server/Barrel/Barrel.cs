using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using Gablarski.Barrel.Config;
using Gablarski.Server;
using log4net;

namespace Gablarski.Barrel
{
	public class Barrel
	{
		public static void Main (string[] args)
		{
			log4net.Config.XmlConfigurator.Configure ();
			
			var config = (BarrelConfiguration)ConfigurationManager.GetSection ("barrel");
			if (config == null)
			{
				LogManager.GetLogger ("Barrel").Fatal ("Section 'barrel' not found in configuration.");
				return;
			}

			foreach (ServerElement serverConfig in config.Servers)
			{
				var log = LogManager.GetLogger (serverConfig.Name);

				log.Info ("Checking configuration");

				Type channelProviderType, authProviderType, permissionProviderType;
				if (!LoadProviderTypes (serverConfig, log, out channelProviderType, out authProviderType, out permissionProviderType))
					continue;

				IEnumerable<Type> connectionProviderTypes;
				LoadConnectionProviders (serverConfig, log, out connectionProviderTypes);
				if (connectionProviderTypes.Count() <= 0)
				{
					log.WarnFormat ("No connection providers found");
					continue;
				}

				log.Info ("Setting up");

				IChannelProvider channels;
				IAuthenticationProvider auth;
				IPermissionsProvider permissions;
				IEnumerable<IConnectionProvider> connectionProviders;

				try
				{
					channels = (IChannelProvider) Activator.CreateInstance (channelProviderType);
					auth = (IAuthenticationProvider) Activator.CreateInstance (authProviderType);
					permissions = (IPermissionsProvider) Activator.CreateInstance (permissionProviderType);

					connectionProviders = connectionProviderTypes.Select (ptype => (IConnectionProvider) Activator.CreateInstance (ptype)).ToList();
				}
				catch (Exception ex)
				{
					log.Warn ("Error while setting up", ex);
					continue;
				}

				GablarskiServer server = new GablarskiServer (new ServerSettings(), auth, permissions, channels);

				foreach (IConnectionProvider provider in connectionProviders)
					server.AddConnectionProvider (provider);

				server.Start();
			}
		}

		static void LoadConnectionProviders (ServerElement serverConfig, ILog log, out IEnumerable<Type> providers)
		{
			List<Type> cproviders = new List<Type>();
			providers = cproviders;

			foreach (ConnectionProviderElement cproviderElement in serverConfig.ConnectionProviders)
			{
				Type provider = Type.GetType (cproviderElement.Type);
				if (provider == null)
				{
					log.WarnFormat ("Connection provider {0} not found", cproviderElement.Type);
					continue;
				}

				cproviders.Add (provider);
			}
		}

		static bool LoadProviderTypes (ServerElement server, ILog log, out Type channelProvider, out Type authProvider, out Type permissionProvider)
		{
			bool errors = false;

			channelProvider = Type.GetType (server.ChannelProvider);
			if (channelProvider == null)
			{
				log.WarnFormat ("Channel provider {0} not found", server.ChannelProvider);
				errors = true;
			}

			authProvider = Type.GetType (server.AuthenticationProvider);
			if (authProvider == null)
			{
				log.WarnFormat ("Authorization provider {0} not found", server.AuthenticationProvider);
				errors = true;
			}

			permissionProvider = Type.GetType (server.PermissionsProvider);
			if (permissionProvider == null)
			{
				log.WarnFormat ("Permissions provider {0} not found", server.PermissionsProvider);
				errors = true;
			}

			return !errors;
		}
	}
}