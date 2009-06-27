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
	public class ServerMessageTests
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

		private const string nickname = "Foo";
		private const int userid = 1;
		private const int channelid = 2;

		[Test]
		public void LoginResult()
		{
			LoginResultState state = LoginResultState.Success;

			var msg = new LoginResultMessage (new LoginResult (userid, state), new UserInfo (nickname, userid, channelid));

			Assert.AreEqual (userid, msg.Result.UserId);
			Assert.AreEqual (state, msg.Result.ResultState);
			Assert.AreEqual (userid, msg.UserInfo.UserId);
			Assert.AreEqual (nickname, msg.UserInfo.Nickname);
			Assert.AreEqual (channelid, msg.UserInfo.CurrentChannelId);
			msg.WritePayload (writer, types);
			long length = stream.Position;
			stream.Position = 0;

			msg = new LoginResultMessage();
			msg.ReadPayload (reader, types);

			Assert.AreEqual (length, stream.Position);
			Assert.AreEqual (userid, msg.Result.UserId);
			Assert.AreEqual (state, msg.Result.ResultState);
			Assert.AreEqual (userid, msg.UserInfo.UserId);
			Assert.AreEqual (nickname, msg.UserInfo.Nickname);
			Assert.AreEqual (channelid, msg.UserInfo.CurrentChannelId);
		}

		[Test]
		public void UserDisconnected()
		{
			object id = 1;
			var msg = new UserDisconnectedMessage (id);
			Assert.AreEqual (id, msg.UserId);

			msg.WritePayload (writer, types);
			long length = stream.Position;
			stream.Position = 0;

			msg = new UserDisconnectedMessage();
			msg.ReadPayload (reader, types);

			Assert.AreEqual (length, stream.Position);
			Assert.AreEqual (id, msg.UserId);
		}
	}
}