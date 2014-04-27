using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace Gablarski.WebServer.Config
{
	public class WebServerConfiguration
		: ConfigurationSection
	{
		[ConfigurationProperty("theme")]
		public ThemeElement Theme
		{
			get { return (ThemeElement)base["theme"]; }
			set { base["theme"] = value; }
		}
	}

	public class ThemeElement
		: ConfigurationElement
	{
		[ConfigurationProperty("path", IsRequired = true)]
		public string Path
		{
			get { return (string)this["path"]; }
			set { this["path"] = value; }
		}
	}
}