using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace Gablarski.Barrel
{
	public class ConnectionProviderElement
		: ConfigurationElement
	{
		[ConfigurationProperty ("type", IsRequired = true)]
		public string Type
		{
			get;
			set;
		}
	}
}