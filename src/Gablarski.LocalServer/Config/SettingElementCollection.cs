using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace Gablarski.LocalServer.Config
{
	public class SettingElementCollection
		: ConfigurationElementCollection, IEnumerable<SettingElement>
	{
		protected override ConfigurationElement CreateNewElement()
		{
			return new SettingElement();
		}

		protected override object GetElementKey (ConfigurationElement element)
		{
			return ((SettingElement) element).Key;
		}

		public new IEnumerator<SettingElement> GetEnumerator()
		{
			var en = base.GetEnumerator();
			en.Reset();

			while (en.MoveNext ())
				yield return (SettingElement) en.Current;
		}
	}
}
