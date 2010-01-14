using System.Collections.Generic;
using System.Configuration;

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

		[ConfigurationProperty ("port", IsRequired = true)]
		public int Port
		{
			get { return (int)this["port"]; }
			set { this["port"] = value; }
		}

		[ConfigurationProperty ("backend", IsRequired=false)]
		public string BackendProvider
		{
			get { return (string)this["backend"]; }
			set { this["backend"] = value; }
		}

		[ConfigurationProperty ("channels", IsRequired = false)]
		public string ChannelProvider
		{
			get { return (string) this["channels"]; }
			set { this["channels"] = value; }
		}

		[ConfigurationProperty ("authentication", IsRequired = false)]
		public string AuthenticationProvider
		{
			get { return (string)this["authentication"]; }
			set { this["authentication"] = value; }
		}

		[ConfigurationProperty ("permissions", IsRequired = false)]
		public string PermissionsProvider
		{
			get { return (string)this["permissions"]; }
			set { this["permissions"] = value; }
		}

		[ConfigurationProperty ("connectionproviders", IsDefaultCollection = true)]
		[ConfigurationCollection (typeof (ConnectionProviderElementCollection), AddItemName="provider")]
		public ConnectionProviderElementCollection ConnectionProviders
		{
			get { return (ConnectionProviderElementCollection) base["connectionproviders"]; }
		}

		protected override ConfigurationElement CreateNewElement()
		{
			return new ConnectionProviderElement();
		}

		protected override object GetElementKey(ConfigurationElement element)
		{
			return ((ConnectionProviderElement)element).Type;
		}
	}
}