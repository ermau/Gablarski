using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentNHibernate.Mapping;
using Gablarski.Clients.Windows.Entities;

namespace Gablarski.Clients.Windows.Mappings
{
	public class SettingMap
		: ClassMap<SettingEntry>
	{
		public SettingMap()
		{
			Id (x => x.ID);
			Map (x => x.Name);
			Map (x => x.Value);
		}
	}
}