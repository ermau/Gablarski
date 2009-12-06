using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;

namespace Gablarski.Barrel
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