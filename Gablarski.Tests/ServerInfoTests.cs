using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Gablarski.Server;
using NUnit.Framework;

namespace Gablarski.Tests
{
	[TestFixture]
	public class ServerInfoTests
	{
		[Test]
		public void InvalidCtor()
		{
			Assert.Throws<ArgumentNullException> (() => new ServerInfo ((ServerSettings)null));
			Assert.Throws<ArgumentNullException> (() => new ServerInfo ((IValueReader)null));
		}

		[Test]
		public void CtorFromSettings()
		{
			const string logo = "logo";
			const string name = "name";
			const string desc = "description";

			ServerInfo info = new ServerInfo (new ServerSettings
			{
				ServerLogo = logo,
				Name = name,
				Description = desc
			});

			Assert.AreEqual (logo, info.ServerLogo);
			Assert.AreEqual (name, info.ServerName);
			Assert.AreEqual (desc, info.ServerDescription);
		}

		[Test]
		public void SerializeDeserialize()
		{
			const string logo = "logo";
			const string name = "name";
			const string desc = "description";

			ServerInfo info = new ServerInfo (new ServerSettings
			{
				ServerLogo = logo,
				Name = name,
				Description = desc
			});

			var stream = new MemoryStream (new byte[20480], true);
			var writer = new StreamValueWriter (stream);
			var reader = new StreamValueReader (stream);

			info.Serialize (writer);
			long length = stream.Position;
			stream.Position = 0;

			info = new ServerInfo (reader);
			Assert.AreEqual (length, stream.Position);
			Assert.AreEqual (logo, info.ServerLogo);
			Assert.AreEqual (name, info.ServerName);
			Assert.AreEqual (desc, info.ServerDescription);
		}
	}
}