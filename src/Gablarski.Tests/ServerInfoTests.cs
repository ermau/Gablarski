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
			Assert.Throws<ArgumentNullException> (() => new ServerInfo ((ServerSettings)null, new GuestUserProvider(), new Decryption().PublicParameters));
			Assert.Throws<ArgumentNullException> (() => new ServerInfo (new ServerSettings(), null, new Decryption().PublicParameters));
			Assert.Throws<ArgumentNullException> (() => new ServerInfo ((IValueReader)null));
		}

		[Test]
		public void CtorFromSettings()
		{
			const string logo = "logo";
			const string name = "name";
			const string desc = "description";
			
			Decryption decryption = new Decryption();

			ServerInfo info = new ServerInfo (new ServerSettings
			{
				ServerLogo = logo,
				Name = name,
				Description = desc,
				ServerPassword = "foo",
			}, new GuestUserProvider(), decryption.PublicParameters);

			Assert.AreEqual (logo, info.Logo);
			Assert.AreEqual (name, info.Name);
			Assert.AreEqual (desc, info.Description);
			Assert.AreEqual (true, info.Passworded);
			Assert.AreEqual (UserRegistrationMode.None, info.RegistrationMode);
			Assert.AreEqual (decryption.PublicParameters, info.PublicRSAParameters);
			Assert.IsNull (info.RegistrationContent);
		}

		[Test]
		public void SerializeDeserialize()
		{
			const string logo = "logo";
			const string name = "name";
			const string desc = "description";

			Decryption decryption = new Decryption();

			ServerInfo info = new ServerInfo (new ServerSettings
			{
				ServerLogo = logo,
				Name = name,
				Description = desc,
				ServerPassword = "passworded"
			}, new GuestUserProvider(), decryption.PublicParameters);

			var stream = new MemoryStream (new byte[20480], true);
			var writer = new StreamValueWriter (stream);
			var reader = new StreamValueReader (stream);

			info.Serialize (writer);
			long length = stream.Position;
			stream.Position = 0;

			info = new ServerInfo (reader);
			Assert.AreEqual (length, stream.Position);
			Assert.AreEqual (logo, info.Logo);
			Assert.AreEqual (name, info.Name);
			Assert.AreEqual (desc, info.Description);
			Assert.AreEqual (true, info.Passworded);
			Assert.AreEqual (UserRegistrationMode.None, info.RegistrationMode);
			Assert.AreEqual (decryption.PublicParameters, info.PublicRSAParameters);
			Assert.IsNull (info.RegistrationContent);
		}
	}
}