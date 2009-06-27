using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Gablarski.Tests
{
	[TestFixture]
	public class UserInfoTests
	{
		private const string nickname = "Foo";
		private const int userid = 1;
		private const int chanid = 2;

		[Test]
		public void InvalidCtor()
		{
			Assert.Throws<ArgumentNullException> (() => new UserInfo (null, new IdentifyingTypes (typeof (Int32), typeof (Int32))));
			Assert.Throws<ArgumentNullException> (() => new UserInfo (new StreamValueReader (new MemoryStream()), null));

			Assert.Throws<ArgumentNullException> (() => new UserInfo (null, userid, chanid));
			Assert.Throws<ArgumentNullException> (() => new UserInfo (nickname, null, chanid));
			Assert.Throws<ArgumentNullException> (() => new UserInfo (nickname, userid, null));
		}

		[Test]
		public void Ctor()
		{
			var info = new UserInfo (nickname, userid, chanid);

			Assert.AreEqual (userid, info.UserId);
			Assert.AreEqual (chanid, info.CurrentChannelId);
			Assert.AreEqual (nickname, info.Nickname);
		}

		[Test]
		public void SerializeDeserialize()
		{
			var stream = new MemoryStream(new byte[20480], true);
			var writer = new StreamValueWriter (stream);
			var reader = new StreamValueReader (stream);
			var types = new IdentifyingTypes (typeof (Int32), typeof (Int32));

			var info = new UserInfo (nickname, userid, chanid);
			info.Serialize (writer, types);
			long length = stream.Position;
			stream.Position = 0;

			info = new UserInfo (reader, types);
			Assert.AreEqual (length, stream.Position);
			Assert.AreEqual (userid, info.UserId);
			Assert.AreEqual (chanid, info.CurrentChannelId);
			Assert.AreEqual (nickname, info.Nickname);
		}
	}
}