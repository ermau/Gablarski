using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Gablarski.Server;
using NUnit.Framework;
using Tempest;

namespace Gablarski.Tests
{
	[TestFixture]
	public class ServerInfoTests
	{
		[Test]
		public void InvalidCtor()
		{
			Assert.Throws<ArgumentNullException> (() => new ServerInfo ((ServerSettings)null, new GuestUserProvider()));
			Assert.Throws<ArgumentNullException> (() => new ServerInfo (new ServerSettings(), null));
			Assert.Throws<ArgumentNullException> (() => new ServerInfo (null, (IValueReader)null));
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
				Description = desc,
				ServerPassword = "foo",
			}, new GuestUserProvider());

			Assert.AreEqual (logo, info.Logo);
			Assert.AreEqual (name, info.Name);
			Assert.AreEqual (desc, info.Description);
			Assert.AreEqual (true, info.Passworded);
			Assert.AreEqual (UserRegistrationMode.None, info.RegistrationMode);
			Assert.IsNull (info.RegistrationContent);
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
				Description = desc,
				ServerPassword = "passworded"
			}, new GuestUserProvider());

			var stream = new MemoryStream (new byte[20480], true);
			var writer = new StreamValueWriter (stream);
			var reader = new StreamValueReader (stream);

			info.Serialize (null, writer);
			long length = stream.Position;
			stream.Position = 0;

			info = new ServerInfo (null, reader);
			Assert.AreEqual (length, stream.Position);
			Assert.AreEqual (logo, info.Logo);
			Assert.AreEqual (name, info.Name);
			Assert.AreEqual (desc, info.Description);
			Assert.AreEqual (true, info.Passworded);
			Assert.AreEqual (UserRegistrationMode.None, info.RegistrationMode);
			Assert.IsNull (info.RegistrationContent);
		}
	}
}