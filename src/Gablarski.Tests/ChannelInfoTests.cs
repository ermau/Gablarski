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
using System.IO;
using System.Linq;
using NUnit.Framework;
using Tempest;

namespace Gablarski.Tests
{
	[TestFixture]
	public class ChannelInfoTests
	{
		public static void AssertChanelsAreEqual (IChannelInfo expected, IChannelInfo actual)
		{
			Assert.AreEqual (expected.ChannelId, actual.ChannelId);
			Assert.AreEqual (expected.ParentChannelId, actual.ParentChannelId);
			Assert.AreEqual (expected.Name, actual.Name);
			Assert.AreEqual (expected.Description, actual.Description);
			Assert.AreEqual (expected.ReadOnly, actual.ReadOnly);
			Assert.AreEqual (expected.UserLimit, actual.UserLimit);
		}

		[Test]
		public void CtorNull()
		{
			Assert.Throws<ArgumentNullException> (() => new ChannelInfo (null));
			Assert.Throws<ArgumentNullException> (() => new ChannelInfo (1, null));
			Assert.Throws<ArgumentNullException> (() => new ChannelInfo (null, null));
		}

		[Test]
		public void CtorCopy()
		{
			var channel = new ChannelInfo (1)
			{
				ParentChannelId = 2,
				Name = "Name",
				Description = "Description",
				ReadOnly = false,
				UserLimit = 2
			};

			var channel2 = new ChannelInfo (channel);

			AssertChanelsAreEqual (channel, channel2);
		}

		[Test]
		public void SerializeDeserialize()
		{
			var stream = new MemoryStream (new byte[20480], true);
			var writer = new StreamValueWriter (stream);
			var reader = new StreamValueReader (stream);

			var channel = new ChannelInfo (1)
			{
				ParentChannelId = 2,
				Name = "Name",
				Description = "Description",
				ReadOnly = false,
				UserLimit = 2
			};

			channel.Serialize (null, writer);
			long length = stream.Position;
			stream.Position = 0;

			var deserializedChannel = new ChannelInfo();
			deserializedChannel.Deserialize (null, reader);

			Assert.AreEqual (length, stream.Position);
			AssertChanelsAreEqual (channel, deserializedChannel);
		}
	}
}