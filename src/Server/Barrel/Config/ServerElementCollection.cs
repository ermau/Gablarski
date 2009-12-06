using System.Collections.Generic;
using System.Configuration;

namespace Gablarski.Barrel.Config
{
	public class ServerElementCollection
		: ConfigurationElementCollection, IEnumerable<ServerElement>
	{
		public new IEnumerator<ServerElement> GetEnumerator ()
		{
			var en = base.GetEnumerator();
			en.Reset ();
			while (en.MoveNext ())
				yield return (ServerElement)en.Current;
		}

		protected override ConfigurationElement CreateNewElement()
		{
			return new ServerElement();
		}

		protected override object GetElementKey (ConfigurationElement element)
		{
			return ((ServerElement)element).Port;
		}
	}
}