using System.Configuration;

namespace Gablarski.Barrel.Config
{
	public class BarrelConfiguration
		: ConfigurationSection
	{
		[ConfigurationProperty ("servers", IsDefaultCollection = true)]
		[ConfigurationCollection (typeof (ServerElementCollection), AddItemName = "server")]
		public ServerElementCollection Servers
		{
			get { return (ServerElementCollection) base["servers"]; }
		}
	}
}