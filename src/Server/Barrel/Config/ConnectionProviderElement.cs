using System.Configuration;

namespace Gablarski.Barrel.Config
{
	public class ConnectionProviderElement
		: ConfigurationElement
	{
		[ConfigurationProperty ("type", IsRequired = true)]
		public string Type
		{
			get { return (string) this["type"]; }
			set { this["type"] = value; }
		}
	}
}