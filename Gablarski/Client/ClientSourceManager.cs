using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gablarski.Client
{
	public class ClientSourceManager
	{
		public ClientSourceManager (IClientConnection connection)
		{
			if (connection == null)
				throw new ArgumentNullException ("connection");

			this.client = connection;
		}

		private readonly IClientConnection client;
	}
}