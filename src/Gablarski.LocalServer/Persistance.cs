// Copyright (c) 2010, Eric Maupin
// All rights reserved.
//
// Redistribution and use in source and binary forms, with
// or without modification, are permitted provided that
// the following conditions are met:
//
// - Redistributions of source code must retain the above 
//   copyright notice, this list of conditions and the
//   following disclaimer.
//
// - Redistributions in binary form must reproduce the above
//   copyright notice, this list of conditions and the
//   following disclaimer in the documentation and/or other
//   materials provided with the distribution.
//
// - Neither the name of Gablarski nor the names of its
//   contributors may be used to endorse or promote products
//   or services derived from this software without specific
//   prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS
// AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED
// WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
// WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR
// PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT
// HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT,
// INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
// (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE
// GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
// INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY,
// WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
// NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
// THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH
// DAMAGE.

using System.Linq;
using NHibernate;
using NHibernate.Cfg;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using NHibernate.Linq;
using NHibernate.Tool.hbm2ddl;

namespace Gablarski.LocalServer
{
	public static class Persistance
	{
		static Persistance()
		{
			SessionFactory = Fluently.Configure ()
				.Database (() => SQLiteConfiguration.Standard.UsingFile (Settings.DatabaseFile.FullName))
				.Mappings (mcfg => mcfg.FluentMappings.AddFromAssemblyOf<LocalUser>())
				.ExposeConfiguration (ExposedConfiguration)
				.BuildSessionFactory();

			using (var session = SessionFactory.OpenSession())
			using (var trans = session.BeginTransaction())
			{
				if (!session.Linq<LocalChannelInfo>().Any())
					ChannelProvider.Setup (session);

				if (!session.Linq<Permission>().Any())
					PermissionProvider.Setup (session);

				trans.Commit();
			}
		}
		
		public static ISessionFactory SessionFactory
		{
			get;
			private set;
		}
		
		private static void ExposedConfiguration (Configuration cfg)
		{
			if (Settings.DatabaseFile.Exists)
				return;
		
			new SchemaExport (cfg).Create (false, true);
		}
	}
}