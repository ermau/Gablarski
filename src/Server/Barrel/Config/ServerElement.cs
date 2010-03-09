// Copyright (c) 2010, Eric Maupin
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
using System.Configuration;
using System.Linq;
using Gablarski.Server;
using log4net;

namespace Gablarski.Barrel.Config
{
	public class ServerElement
		: ConfigurationElementCollection
	{
		[ConfigurationProperty ("name", IsRequired = true)]
		public string Name
		{
			get { return (string) this["name"]; }
			set { this["name"] = value; }
		}

		[ConfigurationProperty ("description", IsRequired = false)]
		public string Description
		{
			get { return (string) this["description"]; }
			set { this["description"] = value; }
		}

		[ConfigurationProperty ("port", DefaultValue = 0)]
		public int Port
		{
			get { return (int)this["port"]; }
			set { this["port"] = value; }
		}

		[ConfigurationProperty ("network", DefaultValue = true)]
		public bool Network
		{
			get
			{
				return (bool)this["network"];
			}

			set
			{
				this["network"] = value;
			}
		}

		[ConfigurationProperty ("providers", IsDefaultCollection = true)]
		[ConfigurationCollection (typeof (ProviderElementCollection), AddItemName="provider")]
		public ProviderElementCollection Providers
		{
			get { return (ProviderElementCollection) base["providers"]; }
		}

		public bool CheckConfiguration (ILog log)
		{
			bool errors = false;
			if (Port == 0 && Network)
			{
				log.Warn ("Port is not set for network provider");
				errors = true;
			}

			if (Port != 0 && !Network)
				log.Warn ("Port is set, but network provider is disabled");

			return !errors;
		}

		public ServerProviders GetProviders (ILog log)
		{
			var providerTypes = Providers.Select (p => Type.GetType (p.Type)).Distinct().Where (t => t != null).ToList();

			Dictionary<Type, object> instances = new Dictionary<Type, object>();

			List<IConnectionProvider> cproviders = new List<IConnectionProvider>();
			foreach (Type cproviderType in providerTypes.Where (t => t.GetInterface ("IConnectionProvider") != null))
			{
				try
				{
					IConnectionProvider cprovider = (IConnectionProvider)Activator.CreateInstance (cproviderType);
					instances.Add (cproviderType, cprovider);

					cproviders.Add (cprovider);
				}
				catch (Exception ex)
				{
					log.Warn ("Error while creating connection provider " + cproviderType.FullName, ex);
				}
			}

			IChannelProvider channels = LoadProvider<IChannelProvider> (providerTypes, instances, log);
			if (channels == null)
				return null;

			IUserProvider auth = LoadProvider<IUserProvider> (providerTypes, instances, log);
			if (auth == null)
				return null;

			IPermissionsProvider permissions = LoadProvider<IPermissionsProvider> (providerTypes, instances, log);
			if (permissions == null)
				return null;

			return new ServerProviders (channels, auth, permissions, cproviders);
		}

		protected override ConfigurationElement CreateNewElement()
		{
			return new ConnectionProviderElement();
		}

		protected override object GetElementKey (ConfigurationElement element)
		{
			return ((ConnectionProviderElement)element).Type;
		}

		private static T LoadProvider<T> (IEnumerable<Type> types, IDictionary<Type, object> instances, ILog log)
			where T : class
		{
			Type contract = typeof (T);

			string name = contract.Name.Substring (1).Substring (0, contract.Name.Length - 1 - 8).ToLower();

			try
			{
				Type implementingType = types.SingleOrDefault (t => t.GetInterface (contract.Name) != null);
				if (implementingType == null)
				{
					log.WarnFormat ("No {0} providers found", name);
					return null;
				}

				object i;
				if (instances.TryGetValue (implementingType, out i))
					return (T)i;

				T instance = (T)Activator.CreateInstance (implementingType);
				instances.Add (implementingType, instance);

				return instance;
			}
			catch (InvalidOperationException)
			{
				log.WarnFormat ("Multiple {0} providers found", name);
				return null;
			}
			catch (Exception ex)
			{
				log.Warn ("Error creating " + name + " provider", ex);
				return null;
			}
		}
	}
}