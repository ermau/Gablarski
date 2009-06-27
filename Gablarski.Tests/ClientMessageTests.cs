using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Gablarski.Messages;
using NUnit.Framework;

namespace Gablarski.Tests
{
	[TestFixture]
	public class ClientMessageTests
	{
		[SetUp]
		public void MessageSetup()
		{
			stream = new MemoryStream(new byte[20480], true);
			writer = new StreamValueWriter (stream);
			reader = new StreamValueReader (stream);
			types = new IdentifyingTypes (typeof (Int32), typeof (Int32));
		}

		[TearDown]
		public void MessageTeardown()
		{
			writer.Dispose();
			reader.Dispose();
		}

		private IdentifyingTypes types;
		private MemoryStream stream;
		private IValueWriter writer;
		private IValueReader reader;

		[Test]
		public void Connect()
		{
			var ver = new Version (1, 2, 32, 4321);
			var msg = new ConnectMessage (ver);
			Assert.AreEqual (ver, msg.ApiVersion);

			msg.WritePayload (writer, types);
			long length = stream.Position;
			stream.Position = 0;

			msg = new ConnectMessage();
			msg.ReadPayload (reader, types);

			Assert.AreEqual (length, stream.Position);
			Assert.AreEqual (ver, msg.ApiVersion);
		}

		[Test]
		public void LoginNoUserNoPass()
		{
			string nickname = "Foo";
			var msg = new LoginMessage { Nickname = nickname };
			msg.WritePayload (writer, types);
			long length = stream.Position;
			stream.Position = 0;

			msg = new LoginMessage();
			msg.ReadPayload (reader, types);

			Assert.AreEqual (length, stream.Position);
			Assert.AreEqual (nickname, msg.Nickname);
		}

		[Test]
		public void Login()
		{
			string nickname = "Foo";
			string username = "foo_";
			string password = "monkeys";

			var msg = new LoginMessage { Nickname = nickname, Username = username, Password = password };
			msg.WritePayload (writer, types);
			long length = stream.Position;
			stream.Position = 0;

			msg = new LoginMessage();
			msg.ReadPayload (reader, types);

			Assert.AreEqual (length, stream.Position);
			Assert.AreEqual (nickname, msg.Nickname);
			Assert.AreEqual (username, msg.Username);
			Assert.AreEqual (password, msg.Password);
		}
	}
}