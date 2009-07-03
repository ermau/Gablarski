using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using Gablarski.Server;

namespace Gablarski.LocalServer
{
	public class LocalUserProvider
		//: IUserProvider
	{
		public LoginResult Login (string username, string password)
		{
			throw new NotImplementedException ();
		}
	}
}