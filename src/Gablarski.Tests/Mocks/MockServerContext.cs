using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gablarski.Server;

namespace Gablarski.Tests.Mocks
{
	public class MockServerContext
		: IServerContext
	{
		public int ProtocolVersion
		{
			get { return GablarskiServer.ProtocolVersion; }
		}

		public IServerUserHandler Users
		{
			get;
			set;
		}

		public IEnumerable<IRedirector> Redirectors
		{
			get;
			set;
		}

		public ServerSettings Settings
		{
			get;
			set;
		}
	}
}