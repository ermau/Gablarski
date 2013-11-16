// Copyright (c) 2010, Eric Maupin
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

		private readonly AudioCodecArgs args = new AudioCodecArgs (AudioFormat.Mono16bitLPCM, 48000, 480, 10);

		private IGablarskiServerContext context;
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
			Assert.AreEqual (args.WaveEncoding, source.WaveEncoding);
			Assert.AreEqual (args.Channels, source.Channels);
			Assert.AreEqual (args.BitsPerSample, source.BitsPerSample);
			Assert.AreEqual (args.Bitrate, source.Bitrate);
			Assert.AreEqual (args.SampleRate, source.SampleRate);
			Assert.AreEqual (args.FrameSize, source.FrameSize);
			Assert.AreEqual (args.Complexity, source.Complexity);

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