// Copyright (c) 2010-2013, Eric Maupin
// All rights reserved.
//
// Redistribution and use in source and binary forms, with
// or without modification, are permitted provided that
// the following conditions are met:
//
// - Redistributions of source code must retain the above 
//   copyright notice, this list of conditions and the
//   following disclaimer.
//
// - Redistributions in binary form must reproduce the above
//   copyright notice, this list of conditions and the
//   following disclaimer in the documentation and/or other
//   materials provided with the distribution.
//
// - Neither the name of Gablarski nor the names of its
//   contributors may be used to endorse or promote products
//   or services derived from this software without specific
//   prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS
// AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED
// WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
// WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR
// PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT
// HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT,
// INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
// (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE
// GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
// INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY,
// WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
// NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
// THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH
// DAMAGE.

using System;
using Gablarski.Audio;
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
			source = new AudioSource ("SourceName", 2, 3, AudioFormat.Mono16bitLPCM, 64000, AudioSourceTests.FrameSize, 10, true);
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