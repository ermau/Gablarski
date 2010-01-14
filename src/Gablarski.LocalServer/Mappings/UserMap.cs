using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentNHibernate.Mapping;
using Gablarski.LocalServer.Entities;

namespace Gablarski.LocalServer.Mappings
{
	public class UserMap
		: ClassMap<LocalUser>
	{
		public UserMap()
		{
			Id (x => x.Id, "userID");
			Map (x => x.UserName, "userName");
			Map (x => x.EncryptedPassword, "userPassword");
		}
	}
}