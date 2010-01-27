using System;
using System.Collections.Generic;
using System.Linq;
using Gablarski.Audio;
using Gablarski.Server;
using Gablarski.Tests.Mocks;
using NUnit.Framework;

namespace Gablarski.Tests
{
	[TestFixture]
	public class ServerSourceManagerTests
	{
		private readonly UserInfo user = new UserInfo ("username", 1, 2, false);
		private readonly UserInfo user2 = new UserInfo ("Username2", 2, 1, false);

		private readonly AudioCodecArgs args = new AudioCodecArgs (1, 64000, 44100, 512, 10);

		private IServerContext context;
		private ServerSourceManager manager;

		[SetUp]
		public void Setup()
		{
			context = new MockServerContext();
			manager = new ServerSourceManager (context);
		}

		[Test]
		public void CreateInvalid()
		{
			Assert.Throws<ArgumentNullException> (() => manager.Create (null, user, new AudioCodecArgs()));
			Assert.Throws<ArgumentNullException> (() => manager.Create ("name", null, new AudioCodecArgs()));
			Assert.Throws<ArgumentNullException> (() => manager.Create ("Name", user, null));
		}

		[Test]
		public void Create()
		{
			AudioSource source = manager.Create ("Name", user, args);

			Assert.IsNotNull (source);
			Assert.AreEqual ("Name", source.Name);
			Assert.AreEqual (1, source.OwnerId);
			Assert.AreEqual (1, source.Channels);
			Assert.AreEqual (64000, source.Bitrate);
			Assert.AreEqual (44100, source.Frequency);
			Assert.AreEqual (512, source.FrameSize);
			Assert.AreEqual (10, source.Complexity);

			Assert.That (source.Id > 0, "Source ID is not greater than 0");
		}

		[Test]
		public void CreateDuplicateForUser()
		{
			manager.Create ("Name", user, args);
			Assert.Throws<ArgumentException> (() => manager.Create ("Name", user, args));
		}

		[Test]
		public void CreateDuplicateNamedSources()
		{
			manager.Create ("Name", user, args);
			Assert.DoesNotThrow (() => manager.Create ("Name", user2, args));
		}

		[Test]
		public void CreateSecond()
		{
			AudioSource source = manager.Create ("Name", user, args);
			AudioSource source2 = manager.Create ("Name2", user, args);

			Assert.AreNotEqual (source.Id, source2.Id, "Subsequent sources should be unique");
		}

		[Test]
		public void IsSourceNameTakenNull()
		{
			Assert.Throws<ArgumentNullException> (() => manager.IsSourceNameTaken (null, "Name"));
			Assert.Throws<ArgumentNullException> (() => manager.IsSourceNameTaken (new UserInfo ("username", 1, 1, true), null));
		}

		[Test]
		public void IsSourceNameTakenForSameUser()
		{
			manager.Create ("SName", user, args);
			Assert.IsTrue (manager.IsSourceNameTaken (user, "SName"));
		}

		[Test]
		public void IsSourceNameTakenForDifferentUser()
		{
			manager.Create ("SName", user, args);
			Assert.IsFalse (manager.IsSourceNameTaken (user2, "SName"));
		}

		[Test]
		public void IsSourceNameNotTakenForSameUser()
		{
			manager.Create ("SName", user, args);
			Assert.IsFalse (manager.IsSourceNameTaken (user, "SName2"));
		}

		[Test]
		public void IsSourceNameNotTakenForDifferentUser()
		{
			manager.Create ("SName", user, args);
			Assert.IsFalse (manager.IsSourceNameTaken (user2, "SName2"));
		}
	}
}