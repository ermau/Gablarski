﻿using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Gablarski.Server;
using Gablarski.Client;
using Gablarski.Messages;

#if MSTEST
using Microsoft.VisualStudio.TestTools.UnitTesting;
#else
using TestInitialize = NUnit.Framework.SetUpAttribute;
using TestCleanup = NUnit.Framework.TearDownAttribute;
using TestClass = NUnit.Framework.TestFixtureAttribute;
using TestMethod = NUnit.Framework.TestAttribute;
using Assert = NUnit.Framework.Assert;
#endif

namespace Gablarski.Tests
{
	[TestClass]
	public class ServerTests
	{
		[TestInitialize]
		public void ServerTestInitialize ()
		{
			this.server = new GablarskiServer (new ServerSettings { Name = "Test Server", Description = "Test Server" }, new GuestUserProvider (), new GuestPermissionProvider(), new LobbyChannelProvider());
			this.server.AddConnectionProvider (this.provider = new MockConnectionProvider ());
			this.server.Start ();
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
		public void TestOldVersionReject ()
		{
			MockServerConnection connection = provider.EstablishConnection ();

			connection.Client.Send (new ConnectMessage (new Version (0,0,0,1)));

			var msg = (connection.Client.DequeueMessage () as ConnectionRejectedMessage);
			Assert.IsNotNull (msg);
			Assert.AreEqual (ConnectionRejectedReason.IncompatibleVersion, msg.Reason);
		}
	}
}
