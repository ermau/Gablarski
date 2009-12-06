using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace Gablarski.Barrel.Config
{
	public class ConnectionProviderElementCollection
		: ConfigurationElementCollection, IEnumerable<ConnectionProviderElement>
	{
		protected override ConfigurationElement CreateNewElement()
		{
			return new ConnectionProviderElement();
		}

		protected override object GetElementKey (ConfigurationElement element)
		{
			return ((ConnectionProviderElement) element).Type;
		}

		public new IEnumerator<ConnectionProviderElement> GetEnumerator()
		{
			var en = base.GetEnumerator();
			en.Reset();

			while (en.MoveNext ())
				yield return (ConnectionProviderElement) en.Current;
		}
	}
}