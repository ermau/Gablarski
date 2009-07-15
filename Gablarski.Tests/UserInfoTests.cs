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
		private const string Username = "Bar";
		private const string Nickname = "Foo";
		private const int UserId = 1;
		private const int ChanId = 2;

		[Test]
		public void InvalidCtor()
		{
			Assert.Throws<ArgumentNullException> (() => new UserInfo (null, new IdentifyingTypes (typeof (Int32), typeof (Int32))));
			Assert.Throws<ArgumentNullException> (() => new UserInfo (new StreamValueReader (new MemoryStream()), null));

			Assert.Throws<ArgumentNullException> (() => new UserInfo (null, null, UserId, ChanId));
			Assert.Throws<ArgumentNullException> (() => new UserInfo (Nickname, null, null, ChanId));
			Assert.Throws<ArgumentNullException> (() => new UserInfo (Nickname, null, UserId, null));
		}

		[Test]
		public void Ctor()
		{
			var info = new UserInfo (Nickname, null, UserId, ChanId);

			Assert.AreEqual (UserId, info.UserId);
			Assert.AreEqual (ChanId, info.CurrentChannelId);
			Assert.AreEqual (Nickname, info.Nickname);
			Assert.AreEqual (Nickname, info.Username);

			info = new UserInfo (Nickname, Username, UserId, ChanId);

			Assert.AreEqual (UserId, info.UserId);
			Assert.AreEqual (ChanId, info.CurrentChannelId);
			Assert.AreEqual (Nickname, info.Nickname);
			Assert.AreEqual (Username, info.Username);
		}

		[Test]
		public void SerializeDeserialize()
		{
			var stream = new MemoryStream(new byte[20480], true);
			var writer = new StreamValueWriter (stream);
			var reader = new StreamValueReader (stream);
			var types = new IdentifyingTypes (typeof (Int32), typeof (Int32));

			var info = new UserInfo (Nickname, null, UserId, ChanId);
			info.Serialize (writer, types);
			long length = stream.Position;
			stream.Position = 0;

			info = new UserInfo (reader, types);
			Assert.AreEqual (length, stream.Position);
			Assert.AreEqual (UserId, info.UserId);
			Assert.AreEqual (ChanId, info.CurrentChannelId);
			Assert.AreEqual (Nickname, info.Nickname);
		}

		[Test]
		public void IsDefault()
		{
			var idtypes = new IdentifyingTypes (typeof (Int32), typeof (Int32));
			Assert.IsTrue (UserInfo.IsDefault (default (Int32), idtypes));
			Assert.IsFalse (UserInfo.IsDefault (1, idtypes));
		}

		[Test]
		public void Equals()
		{
			UserInfo foo = new UserInfo ("foo", null, 1, 2);
			UserInfo bar = new UserInfo ("foo", null, 1, 2);

			Assert.AreEqual (foo, bar);
		}

		[Test]
		public void HashCode()
		{
			UserInfo foo = new UserInfo ("foo", null, 1, 2);
			UserInfo bar = new UserInfo ("foo", null, 1, 2);

			Assert.AreEqual (foo.GetHashCode(), bar.GetHashCode());
		}
	}
}