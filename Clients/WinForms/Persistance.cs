using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Tool.hbm2ddl;

namespace Gablarski.Clients.Windows
{
	public static class Persistance
	{
		public static ISession CurrentSession
		{
			get
			{
				lock (DbFile)
				{
		            if (currentSession == null)
		                currentSession = CreateSessionFactory().OpenSession();

		            return currentSession;
		        }
		    }
		}

		private static ISession currentSession;

		private static readonly FileInfo DbFile = new FileInfo (Path.Combine (System.Environment.GetFolderPath (System.Environment.SpecialFolder.ApplicationData), "Gablarski\\gablarski.db"));
		private static ISessionFactory CreateSessionFactory()
		{
			return Fluently.Configure()
				.Database (SQLiteConfiguration.Standard.UsingFile (DbFile.FullName))
				.Mappings (m => m.FluentMappings.AddFromAssembly (Assembly.GetExecutingAssembly()))
				.ExposeConfiguration (BuildSchema)
				.BuildSessionFactory();
		}

		private static void BuildSchema (Configuration config)
		{
			if (!DbFile.Directory.Exists)
				DbFile.Directory.Create();

			if (DbFile.Exists)
				return;

			new SchemaExport (config).Create (false, true);
		}
	}
}