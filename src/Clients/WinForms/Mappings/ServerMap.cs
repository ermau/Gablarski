using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentNHibernate.Mapping;
using Gablarski.Clients.Windows.Entities;

namespace Gablarski.Clients.Windows.Mappings
{
	public class ServerMap
		: ClassMap<ServerEntry>
	{
		public ServerMap()
		{
			Id (x => x.Id);
			Map (x => x.Name);
			Map (x => x.Host);
			Map (x => x.Port);
			Map (x => x.ServerPassword);
			
			Map (x => x.UserNickname);
			Map (x => x.UserPhonetic);
			Map (x => x.UserName);
			Map (x => x.UserPassword);
		}
	}
}