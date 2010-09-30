using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace Gablarski.LocalServer.Config
{
	public class SettingElement
		: ConfigurationElement
	{
		[ConfigurationProperty ("key", IsRequired = true)]
		public string Key
		{
			get { return (string)this["key"]; }
			set { this["key"] = value; }
		}

		[ConfigurationProperty ("value", IsRequired = true)]
		public string Value
		{
			get { return (string)this["value"]; }
			set { this["value"] = value; }
		}
	}
}