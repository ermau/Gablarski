using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gablarski.Clients.Windows.Entities;

namespace Gablarski.Clients.Windows
{
	public static class Servers
	{
		public static IEnumerable<ServerEntry> GetEntries()
		{
			return Persistance.CurrentSession.CreateQuery ("from ServerEntry").Enumerable().Cast<ServerEntry>();
		}

		public static void SaveServer (ServerEntry entry)
		{
			Persistance.CurrentSession.SaveOrUpdate (entry);
		}

		public static void DeleteServer (ServerEntry entry)
		{
			Persistance.CurrentSession.Delete (entry);
		}
	}
}