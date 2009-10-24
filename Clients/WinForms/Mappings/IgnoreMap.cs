using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gablarski.Clients.Windows.Entities;
using FluentNHibernate.Mapping;

namespace Gablarski.Clients.Windows.Mappings
{
	public class IgnoreMap
		: ClassMap<IgnoreEntry>
	{
		public IgnoreMap ()
		{
			Id (x => x.Id);
			Map (x => x.ServerId);
			Map (x => x.Username);
		}
	}
}