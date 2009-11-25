using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;
using System.Linq;

namespace Gablarski.Barrel
{
	public class ServersElement
		: ConfigurationElementCollection, IEnumerable<ServerElement>
	{
		public new IEnumerator<ServerElement> GetEnumerator ()
		{
			throw new NotImplementedException();
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