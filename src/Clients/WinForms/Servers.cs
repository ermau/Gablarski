using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gablarski.Clients.Persistence;

namespace Gablarski.Clients.Windows
{
	public static class Servers
	{
		public static IEnumerable<ServerEntry> GetEntries()
		{
			return Persistence.Persistence.GetServers();
		}

		public static void SaveServer (ServerEntry entry)
		{
			Persistence.Persistence.SaveOrUpdate (entry);
		}

		public static void DeleteServer (ServerEntry entry)
		{
			Persistence.Persistence.Delete (entry);
		}
	}
}