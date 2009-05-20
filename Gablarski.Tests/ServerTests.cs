using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Gablarski.Server;
using Gablarski.Client;

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

		#region Additional test attributes
		//
		// You can use the following additional attributes as you write your tests:
		//
		// Use ClassInitialize to run code before running the first test in the class
		// [ClassInitialize()]
		// public static void MyClassInitialize(TestContext testContext) { }
		//
		// Use ClassCleanup to run code after all tests in a class have run
		// [ClassCleanup()]
		// public static void MyClassCleanup() { }
		//
		// Use TestInitialize to run code before running each test 
		// [TestInitialize()]
		// public void MyTestInitialize() { }
		//
		// Use TestCleanup to run code after each test has run
		// [TestCleanup()]
		// public void MyTestCleanup() { }
		//
		#endregion

		[TestInitialize]
		public void ServerTestInitialize ()
		{
			this.server = new GablarskiServer (new ServerInfo { ServerName = "Test Server", ServerDescription = "Test Server" }, new GuestUserProvider ());
			this.server.AddConnectionProvider (new MockConnectionProvider ());
		}

		private GablarskiServer server;

		[TestMethod]
		public void TestConnection ()
		{
			
		}
	}
}
