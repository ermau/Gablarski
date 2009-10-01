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
		}

		[TearDown]
		public void MessageTeardown()
		{
			writer.Dispose();
			reader.Dispose();
		}

		private MemoryStream stream;
		private IValueWriter writer;
		private IValueReader reader;

		[Test]
		public void Connect()
		{
			var ver = new Version (1, 2, 32, 4321);
			var msg = new ConnectMessage (ver);
			Assert.AreEqual (ver, msg.ApiVersion);

			msg.WritePayload (writer);
			long length = stream.Position;
			stream.Position = 0;

			msg = new ConnectMessage();
			msg.ReadPayload (reader);

			Assert.AreEqual (length, stream.Position);
			Assert.AreEqual (ver, msg.ApiVersion);
		}

		[Test]
		public void Join()
		{
			string nickname = "Foo";
			var msg = new JoinMessage { Nickname = nickname };
			msg.WritePayload (writer);
			long length = stream.Position;
			stream.Position = 0;

			msg = new JoinMessage();
			msg.ReadPayload (reader);

			Assert.AreEqual (length, stream.Position);
			Assert.AreEqual (nickname, msg.Nickname);
		}

		
		[Test]
		public void JoinWithServerPassword()
		{
			string nickname = "Foo";
			string password = "pass";
			var msg = new JoinMessage (nickname, password);
			Assert.AreEqual (nickname, msg.Nickname);
			Assert.AreEqual (password, msg.ServerPassword);
			msg.WritePayload (writer);
			long length = stream.Position;
			stream.Position = 0;

			msg = new JoinMessage();
			msg.ReadPayload (reader);

			Assert.AreEqual (length, stream.Position);
			Assert.AreEqual (nickname, msg.Nickname);
			Assert.AreEqual (password, msg.ServerPassword);
		}

		[Test]
		public void Login()
		{
			string username = "foo_";
			string password = "monkeys";

			var msg = new LoginMessage { Username = username, Password = password };
			msg.WritePayload (writer);
			long length = stream.Position;
			stream.Position = 0;

			msg = new LoginMessage();
			msg.ReadPayload (reader);

			Assert.AreEqual (length, stream.Position);
			Assert.AreEqual (username, msg.Username);
			Assert.AreEqual (password, msg.Password);
		}

		[Test]
		public void RequestSource()
		{
			string name = "Voice";
			int bitrate = 64000;
			short frameSize = 512;
			int channels = 1;

			var msg = new RequestSourceMessage (name, channels, bitrate, frameSize);
			Assert.AreEqual (name, msg.Name);
			Assert.AreEqual (bitrate, msg.TargetBitrate);
			Assert.AreEqual (frameSize, msg.FrameSize);
			Assert.AreEqual (channels, msg.Channels);
			msg.WritePayload (writer);
			long length = stream.Position;
			stream.Position = 0;

			msg = new RequestSourceMessage();
			msg.ReadPayload (reader);
			Assert.AreEqual (length, stream.Position);
			Assert.AreEqual (name, msg.Name);
			Assert.AreEqual (bitrate, msg.TargetBitrate);
			Assert.AreEqual (frameSize, msg.FrameSize);
			Assert.AreEqual (channels, msg.Channels);
		}
	}
}