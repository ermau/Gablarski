using System;
using System.Configuration;
using System.Collections.Generic;
using Gablarski.Server;
using System.Linq;
using FluentNHibernate.Cfg.Db;

namespace Gablarski.Barrel
{
	public class BarrelConfiguration
		: ConfigurationSection
	{
		public BarrelConfiguration ()
		{
			//IPersistenceConfigurer foo;
		}
		
		public IEnumerable<ServerSettings> Servers
		{
			get; set;
		}
	}
}
