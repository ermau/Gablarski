using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gablarski.Client;

namespace Gablarski.Server
{
	public class GablarskiServer
	{
		public GablarskiServer (int port)
		{
			
		}

		public IEnumerable<IConnection> Connections
		{
			get
			{
				lock (connectionLock)
				{
					return this.connections.ToArray ();
				}
			}

			set
			{
				this.connections = value.ToList();
			}
		}

		private object connectionLock = new object();
		private List<IConnection> connections;
	}
}