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
using Gablarski.Client;
using Gablarski.Messages;
using NUnit.Framework;

namespace Gablarski.Tests
{
	[TestFixture]
	public class ClientSourceManagerTests
	{
		[SetUp]
		public void ManagerSetup()
		{
			this.provider = new MockConnectionProvider();
			this.server = this.provider.EstablishConnection();
			this.client = this.server.Client;

			context = new MockClientContext { Connection = this.server.Client };
			var channels = new ClientChannelManager (context);
			ClientChannelManagerTests.PopulateChannels (channels, server);

			context.Users = new ClientUserHandler (context, new ClientUserManager());
			context.Channels = channels;
			context.CurrentUser = new CurrentUser (context, 1, "Foo", channels.First().ChannelId);

			this.manager = new ClientSourceManager (context);
		}

		private IEnumerable<AudioSource> CreateSources()
		{
			var source = AudioSourceTests.GetTestSource (1, 0);
			manager.Add (source);

			var source2 = AudioSourceTests.GetTestSource (2, 1);
			manager.Add (source2);

			return new[] { source, source2 };
		}

		private ClientSourceManager manager;
		private MockConnectionProvider provider;
		private MockServerConnection server;
		private MockClientConnection client;
		private MockClientContext context;

		[Test]
		public void NullContext()
		{
			Assert.Throws<ArgumentNullException> (() => new ClientSourceManager (null));
		}

		[Test]
		public void AddNull()
		{
			Assert.Throws<ArgumentNullException> (() => manager.Add (null));
		}

		[Test]
		public void Add()
		{
			CreateSources();

			var csource = manager.FirstOrDefault (s => s.Id == 1);
			Assert.IsNotNull (csource, "Source not found");
			AudioSourceTests.AssertSourcesMatch (AudioSourceTests.GetTestSource (1, 0), csource);
			Assert.IsTrue (manager[context.CurrentUser].Contains (csource), "Owned sources did not contain source");

			var source = manager.FirstOrDefault (s => s.Id == 2);
			Assert.IsNotNull (source, "Source not found");
			AudioSourceTests.AssertSourcesMatch (AudioSourceTests.GetTestSource (2, 1), source);
		}

		[Test]
		public void UpdateSourceNull()
		{
			Assert.Throws<ArgumentNullException> (() => manager.Update ((AudioSource) null));
		}

		[Test]
		public void UpdateSourcesNull()
		{
			Assert.Throws<ArgumentNullException> (() => manager.Update ((IEnumerable<AudioSource>) null));
		}
		
		//[Test]
		//public void UpdateSourcesRemove()
		//{
		//    CreateSources();


		//}

		[Test]
		public void ToggleIgnoreNull()
		{
			Assert.Throws<ArgumentNullException> (() => manager.ToggleIgnore (null));
		}

		[Test]
		public void ToggleIgnore()
		{
			CreateSources();

			var source = manager.First();
			Assert.IsFalse (manager.GetIsIgnored (source));
			Assert.IsTrue (manager.ToggleIgnore (source));
			Assert.IsTrue (manager.GetIsIgnored (source));
			Assert.IsFalse (manager.ToggleIgnore (source));
			Assert.IsFalse (manager.GetIsIgnored (source));
		}

		[Test]
		public void ToggleIgnoreUnknownSource()
		{
			var source = AudioSourceTests.GetTestSource();
			Assert.IsFalse (manager.GetIsIgnored (source));
			Assert.IsFalse (manager.ToggleIgnore (source));
			Assert.IsFalse (manager.GetIsIgnored (source));
		}

		[Test]
		public void ToggleIgnorePersisted()
		{
			CreateSources();

			var source = manager.First();
			Assert.IsFalse (manager.GetIsIgnored (source));
			Assert.IsTrue (manager.ToggleIgnore (source));
			Assert.IsTrue (manager.GetIsIgnored (source));

			CreateSources();

			Assert.IsTrue (manager.GetIsIgnored (source));
			Assert.IsFalse (manager.ToggleIgnore (source));
			Assert.IsFalse (manager.GetIsIgnored (source));
		}

		[Test]
		public void ToggleMuteNull()
		{
			Assert.Throws<ArgumentNullException> (() => manager.ToggleMute (null));
		}
	}
}