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
	public class SourceManagerTests
	{
		private AudioSource source;
		private MockSourceManager manager;

		[SetUp]
		public void Setup()
		{
			source = new AudioSource ("SourceName", 2, 3, AudioFormat.Mono16bitLPCM, 64000, 512, 10, true);
			manager = new MockSourceManager();
		}

		[TearDown]
		public void Teardown()
		{
			source = null;

			manager = null;
		}

		[Test]
		public void AddNull()
		{
			Assert.Throws<ArgumentNullException> (() => manager.Add (null));
		}

		[Test]
		public void ToggleMuteNull()
		{
			Assert.Throws<ArgumentNullException> (() => manager.ToggleMute (null));
		}

		[Test]
		public void ToggleMuted()
		{
			Assert.IsTrue (source.IsMuted);
			manager.Add (source);

			Assert.IsFalse (manager.ToggleMute (source));

			var s = manager[source.Id];
			Assert.IsNotNull (s);
			Assert.IsFalse (s.IsMuted);
		}

		[Test]
		public void ToggleUnmuted()
		{
			source.IsMuted = false;
			Assert.IsFalse (source.IsMuted);
			manager.Add (source);

			Assert.IsTrue (manager.ToggleMute (source));

			var s = manager[source.Id];
			Assert.IsNotNull (s);
			Assert.IsTrue (s.IsMuted);
		}

		[Test]
		public void ToggleIgnoreReferenceChanges()
		{
			manager.Add (source);
			source.Name = "NewName";

			Assert.IsFalse (manager.ToggleMute (source));

			Assert.AreNotEqual (source.Name, manager[source.Id].Name);
		}

		[Test]
		public void ToggleIgnoreChanges()
		{
			manager.Add (source);

			var update = new AudioSource (source);
			update.Name = "NewName";

			Assert.IsFalse (manager.ToggleMute (update));

			Assert.AreNotEqual (update.Name, manager[update.Id].Name);
		}

		[Test]
		public void RemoveNull()
		{
			Assert.Throws<ArgumentNullException> (() => manager.Remove ((AudioSource)null));
			Assert.Throws<ArgumentNullException> (() => manager.Remove ((UserInfo)null));
		}
	}
}