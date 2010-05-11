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
using System.IO;
using System.Linq;
using Gablarski.Audio;
using NUnit.Framework;

namespace Gablarski.Tests
{
	[TestFixture]
	public class AudioSourceTests
	{
		[Test]
		public void InvalidSource()
		{
			Assert.Throws<ArgumentNullException> (() => new AudioSource (null, 1, 1, AudioFormat.Mono16bitLPCM, 64000, 512, 10, false));

			Assert.Throws<ArgumentOutOfRangeException> (() => new AudioSource ("voice", 0, 1, AudioFormat.Mono16bitLPCM, 64000, 512, 10, false));

			Assert.Throws<ArgumentException> (() => new AudioSource ("voice", 1, 0, AudioFormat.Mono16bitLPCM, 64000, 512, 10, false));

			Assert.Throws<ArgumentOutOfRangeException> (() => new AudioSource ("voice", 1, 1, AudioFormat.Mono16bitLPCM, -1, 512, 10, false));

			Assert.Throws<ArgumentOutOfRangeException> (() => new AudioSource ("voice", 1, 1, AudioFormat.Mono16bitLPCM, 64000, 32, 10, false));
			Assert.Throws<ArgumentOutOfRangeException> (() => new AudioSource ("voice", 1, 1, AudioFormat.Mono16bitLPCM, 64000, 5120, 10, false));

			Assert.Throws<ArgumentOutOfRangeException> (() => new AudioSource ("voice", 1, 1, AudioFormat.Mono16bitLPCM, 64000, 512, 0, false));
			Assert.Throws<ArgumentOutOfRangeException> (() => new AudioSource ("voice", 1, 1, AudioFormat.Mono16bitLPCM, 64000, 512, 11, false));
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
			return new AudioSource ("TestSource" + increment, 1 + increment, owner, AudioFormat.Mono16bitLPCM, 64000, 256, 10, false);
		}

		public static void AssertSourcesMatch (AudioSource expected, AudioSource actual)
		{
			Assert.AreEqual (expected.Name, actual.Name, "Source Name not matching");
			Assert.AreEqual (expected.OwnerId, actual.OwnerId, "Source OwnerId not matching");
			Assert.AreEqual (expected.WaveEncoding, actual.WaveEncoding, "Source WaveEncoding not matching");
			Assert.AreEqual (expected.Channels, actual.Channels, "Source Channels not matching");
			Assert.AreEqual (expected.BitsPerSample, actual.BitsPerSample, "Source BitsPerSample not matching");
			Assert.AreEqual (expected.Bitrate, actual.Bitrate, "Source Bitrate not matching");
			Assert.AreEqual (expected.SampleRate, actual.SampleRate, "Source Frequency not matching");
			Assert.AreEqual (expected.FrameSize, actual.FrameSize, "Source FrameSize not matching");
			Assert.AreEqual (expected.Complexity, actual.Complexity, "Source Complexity not matching.");
			Assert.AreEqual (expected.IsMuted, actual.IsMuted, "Source IsMuted not matching");
		}

		[Test]
		public void Values()
		{
			var source = new AudioSource ("voice", 1, 1, new AudioFormat (WaveFormatEncoding.LPCM, 2, 8, 48000), 64000, 512, 10, true);
			Assert.AreEqual ("voice",					source.Name);
			Assert.AreEqual (1,							source.Id);
			Assert.AreEqual (WaveFormatEncoding.LPCM,	source.WaveEncoding);
			Assert.AreEqual (2,							source.Channels);
			Assert.AreEqual (8,							source.BitsPerSample);
			Assert.AreEqual (1,							source.OwnerId);
			Assert.AreEqual (64000,						source.Bitrate);
			Assert.AreEqual (48000,						source.SampleRate);
			Assert.AreEqual (512,						source.FrameSize);
			Assert.AreEqual (10,						source.Complexity);
			Assert.AreEqual (true,						source.IsMuted);
		}

		[Test]
		public void SerializeDeserialize()
		{
			var stream = new MemoryStream (new byte[20480], true);
			var writer = new StreamValueWriter (stream);
			var reader = new StreamValueReader (stream);

			var source = GetTestSource();
			source.Serialize (writer);
			long length = stream.Position;
			stream.Position = 0;

			source = new AudioSource (reader);
			AssertSourcesMatch (GetTestSource(), source);
			Assert.AreEqual (length, stream.Position);
		}
	}
}