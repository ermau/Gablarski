using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Gablarski.Server;
using Gablarski.Client;
using Gablarski.Messages;

namespace Gablarski.Tests
{
	[TestClass]
	public class ServerTests
	{
		/// <summary>
		///Gets or sets the test context which provides
		///information about and functionality for the current test run.
		///</summary>
		public TestContext TestContext
		{
			get;
			set;
		}

		[TestInitialize]
		public void ServerTestInitialize ()
		{
			this.server = new GablarskiServer (new ServerInfo { ServerName = "Test Server", ServerDescription = "Test Server" }, new GuestUserProvider (), new GuestPermissionProvider(), new LobbyChannelProvider());
			this.server.AddConnectionProvider (this.provider = new MockConnectionProvider ());
		}

		[TestCleanup]
		public void ServerTestCleanup ()
		{
			this.server.Shutdown ();
			this.server = null;
			this.provider = null;
		}

		private GablarskiServer server;
		private MockConnectionProvider provider;

		[TestMethod]
		public void TestConnection ()
		{
			IConnection connection = provider.EstablishConnection ();
		}

		[TestMethod]
		public void TestGarbageLogin ()
		{
			IConnection connection = provider.EstablishConnection ();
			connection.Send (new LoginMessage { Nickname = null, Username = null });
		}
	}
}
