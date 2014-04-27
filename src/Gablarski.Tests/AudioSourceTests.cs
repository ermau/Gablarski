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
using System.IO;
using System.Linq;
using Gablarski.Audio;
using NUnit.Framework;
using Tempest;

namespace Gablarski.Tests
{
	[TestFixture]
	public class AudioSourceTests
	{
		public const int FrameSize = 480;

		[Test]
		public void InvalidSource()
		{
			Assert.Throws<ArgumentNullException> (() => new AudioSource (null, 1, 1, AudioFormat.Mono16bitLPCM, 64000, 480, 10, false));

			Assert.Throws<ArgumentOutOfRangeException> (() => new AudioSource ("voice", 0, 1, AudioFormat.Mono16bitLPCM, 64000, 480, 10, false));

			Assert.Throws<ArgumentException> (() => new AudioSource ("voice", 1, 0, AudioFormat.Mono16bitLPCM, 64000, 480, 10, false));

			Assert.Throws<ArgumentOutOfRangeException> (() => new AudioSource ("voice", 1, 1, AudioFormat.Mono16bitLPCM, -1, 480, 10, false));

			Assert.Throws<ArgumentOutOfRangeException> (() => new AudioSource ("voice", 1, 1, AudioFormat.Mono16bitLPCM, 64000, 32, 10, false));
			Assert.Throws<ArgumentOutOfRangeException> (() => new AudioSource ("voice", 1, 1, AudioFormat.Mono16bitLPCM, 64000, 5120, 10, false));

			Assert.Throws<ArgumentOutOfRangeException> (() => new AudioSource ("voice", 1, 1, AudioFormat.Mono16bitLPCM, 64000, 480, 0, false));
			Assert.Throws<ArgumentOutOfRangeException> (() => new AudioSource ("voice", 1, 1, AudioFormat.Mono16bitLPCM, 64000, 480, 11, false));
		}

		public static AudioSource GetTestSource ()
		{
			return GetTestSource (1, 1);
		}

		public static AudioSource GetTestSource (int owner)
		{
			return GetTestSource (owner, 1);
		}

		public static AudioSource GetTestSource (int owner, int increment)
		{
			return new AudioSource ("TestSource" + increment, 1 + increment, owner, AudioFormat.Mono16bitLPCM, 48000, 240, 10, false);
		}

		public static void AssertSourcesMatch (AudioSource expected, AudioSource actual)
		{
			Assert.AreEqual (expected.Name, actual.Name, "Source Name not matching");
			Assert.AreEqual (expected.OwnerId, actual.OwnerId, "Source OwnerId not matching");
			Assert.AreEqual (expected.CodecSettings.WaveEncoding, actual.CodecSettings.WaveEncoding, "Source WaveEncoding not matching");
			Assert.AreEqual (expected.CodecSettings.Channels, actual.CodecSettings.Channels, "Source Channels not matching");
			Assert.AreEqual (expected.CodecSettings.BitsPerSample, actual.CodecSettings.BitsPerSample, "Source BitsPerSample not matching");
			Assert.AreEqual (expected.CodecSettings.Bitrate, actual.CodecSettings.Bitrate, "Source Bitrate not matching");
			Assert.AreEqual (expected.CodecSettings.SampleRate, actual.CodecSettings.SampleRate, "Source Frequency not matching");
			Assert.AreEqual (expected.CodecSettings.FrameSize, actual.CodecSettings.FrameSize, "Source FrameSize not matching");
			Assert.AreEqual (expected.CodecSettings.Complexity, actual.CodecSettings.Complexity, "Source Complexity not matching.");
			Assert.AreEqual (expected.IsMuted, actual.IsMuted, "Source IsMuted not matching");
		}

		[Test]
		public void Values()
		{
			var source = new AudioSource ("voice", 1, 1, new AudioFormat (WaveFormatEncoding.LPCM, 2, 8, 48000), 64000, 480, 10, true);
			Assert.AreEqual ("voice",					source.Name);
			Assert.AreEqual (1,							source.Id);
			Assert.AreEqual (1,							source.OwnerId);
			Assert.AreEqual (true,						source.IsMuted);
			Assert.AreEqual (WaveFormatEncoding.LPCM,	source.CodecSettings.WaveEncoding);
			Assert.AreEqual (2,							source.CodecSettings.Channels);
			Assert.AreEqual (8,							source.CodecSettings.BitsPerSample);
			Assert.AreEqual (64000,						source.CodecSettings.Bitrate);
			Assert.AreEqual (48000,						source.CodecSettings.SampleRate);
			Assert.AreEqual (480,						source.CodecSettings.FrameSize);
			Assert.AreEqual (10,						source.CodecSettings.Complexity);
			
		}

		[Test]
		public void SerializeDeserialize()
		{
			var stream = new MemoryStream (new byte[20480], true);
			var writer = new StreamValueWriter (stream);
			var reader = new StreamValueReader (stream);

			var source = GetTestSource();
			source.Serialize (null, writer);
			long length = stream.Position;
			stream.Position = 0;

			source = new AudioSource (null, reader);
			AssertSourcesMatch (GetTestSource(), source);
			Assert.AreEqual (length, stream.Position);
		}
	}
}