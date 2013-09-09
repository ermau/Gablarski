using System.Collections.Generic;
using Gablarski.Clients.Persistence;

namespace Gablarski.Clients.Windows
{
	public static class Servers
	{
		public static IEnumerable<ServerEntry> GetEntries()
		{
			return ClientData.GetServers();
		}

		public static void SaveServer (ServerEntry entry)
		{
			ClientData.SaveOrUpdate (entry);
		}

		public static void DeleteServer (ServerEntry entry)
		{
			ClientData.Delete (entry);
		}
	}
}