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
using Gablarski.Audio;
using NUnit.Framework;

namespace Gablarski.Tests
{
	[TestFixture]
	public class AudioCodecArgsTests
	{
		

		[Test]
		public void InvalidParamsCtor ()
		{
			Assert.Throws<ArgumentOutOfRangeException> (() => new AudioCodecArgs (0, 48000, 44100, 512, 10));
			Assert.Throws<ArgumentOutOfRangeException> (() => new AudioCodecArgs (11, 48000, 44100, 512, 10));

			Assert.Throws<ArgumentOutOfRangeException> (() => new AudioCodecArgs (1, -1, 44100, 512, 10));
			Assert.Throws<ArgumentOutOfRangeException> (() => new AudioCodecArgs (1, 480000, 44100, 512, 10));

			Assert.Throws<ArgumentOutOfRangeException> (() => new AudioCodecArgs (1, 48000, 5, 512, 10));
			Assert.Throws<ArgumentOutOfRangeException> (() => new AudioCodecArgs (1, 48000, 200000, 512, 10));

			Assert.Throws<ArgumentOutOfRangeException> (() => new AudioCodecArgs (1, 48000, 44100, 1, 10));
			Assert.Throws<ArgumentOutOfRangeException> (() => new AudioCodecArgs (1, 48000, 44100, 5120, 10));

			Assert.Throws<ArgumentOutOfRangeException> (() => new AudioCodecArgs (1, 48000, 44100, 512, 0));
			Assert.Throws<ArgumentOutOfRangeException> (() => new AudioCodecArgs (11, 48000, 44100, 512, 10));
		}

		[Test]
		public void InvalidArgsCtor()
		{
			Assert.Throws<ArgumentNullException> (() => new AudioCodecArgs ((AudioCodecArgs)null));
			Assert.Throws<ArgumentNullException> (() => new AudioCodecArgs ((IValueReader)null));
		}

		private const int Bitrate = 64000;
		private const byte Channels = 1;
		private const int Frequency = 44100;
		private const int FrameSize = 512;
		private const byte Complexity = 10;

		public static void AssertAreEqual (AudioCodecArgs expected, AudioCodecArgs actual)
		{
			Assert.AreEqual (expected.Bitrate, actual.Bitrate);
			Assert.AreEqual (expected.Channels, actual.Channels);
			Assert.AreEqual (expected.Complexity, actual.Complexity);
			Assert.AreEqual (expected.FrameSize, actual.FrameSize);
			Assert.AreEqual (expected.Frequency, actual.Frequency);
		}
		
		[Test]
		public void Ctor()
		{
			var args = new AudioCodecArgs (Channels, Bitrate, Frequency, FrameSize, Complexity);
			Assert.AreEqual (Channels, args.Channels);
			Assert.AreEqual (Bitrate, args.Bitrate);
			Assert.AreEqual (Frequency, args.Frequency);
			Assert.AreEqual (FrameSize, args.FrameSize);
			Assert.AreEqual (Complexity, args.Complexity);
		}

		[Test]
		public void SerializeDeserialize()
		{
			var stream = new MemoryStream(new byte[20480], true);
			var writer = new StreamValueWriter (stream);
			var reader = new StreamValueReader (stream);

			var args = new AudioCodecArgs (Channels, Bitrate, Frequency, FrameSize, Complexity);
			
			args.Serialize (writer);
			long length = stream.Position;
			stream.Position = 0;

			args = new AudioCodecArgs (reader);
			Assert.AreEqual (length, stream.Position);
			Assert.AreEqual (Channels, args.Channels);
			Assert.AreEqual (Bitrate, args.Bitrate);
			Assert.AreEqual (Frequency, args.Frequency);
			Assert.AreEqual (FrameSize, args.FrameSize);
			Assert.AreEqual (Complexity, args.Complexity);
		}
	}
}