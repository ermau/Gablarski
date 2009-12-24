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
			return Persistance.GetServers();
		}

		public static void SaveServer (ServerEntry entry)
		{
			Persistance.SaveOrUpdate (entry);
		}

		public static void DeleteServer (ServerEntry entry)
		{
			Persistance.Delete (entry);
		}
	}
}