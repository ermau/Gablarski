using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
			Assert.Throws<ArgumentNullException> (() => new AudioSource (null, 1, 1, 1, 64000, 44100, 512, 10, false));

			Assert.Throws<ArgumentOutOfRangeException> (() => new AudioSource ("voice", 0, 1, 1, 64000, 44100, 512, 10, false));

			Assert.Throws<ArgumentException> (() => new AudioSource ("voice", 1, 0, 1, 64000, 44100, 512, 10, false));

			Assert.Throws<ArgumentOutOfRangeException> (() => new AudioSource ("voice", 1, 1, 0, 0, 41000, 512, 10, false));
			Assert.Throws<ArgumentOutOfRangeException> (() => new AudioSource ("voice", 1, 1, 3, 0, 41000, 512, 10, false));

			Assert.Throws<ArgumentOutOfRangeException> (() => new AudioSource ("voice", 1, 1, 1, -1, 41000, 512, 10, false));

			Assert.Throws<ArgumentOutOfRangeException> (() => new AudioSource ("voice", 1, 1, 1, 64000, 0, 512, 10, false));
			Assert.Throws<ArgumentOutOfRangeException> (() => new AudioSource ("voice", 1, 1, 1, 64000, 100000, 512, 10, false));

			Assert.Throws<ArgumentOutOfRangeException> (() => new AudioSource ("voice", 1, 1, 1, 64000, 44100, 32, 10, false));
			Assert.Throws<ArgumentOutOfRangeException> (() => new AudioSource ("voice", 1, 1, 1, 64000, 44100, 5120, 10, false));

			Assert.Throws<ArgumentOutOfRangeException> (() => new AudioSource ("voice", 1, 1, 1, 64000, 100000, 512, 0, false));
			Assert.Throws<ArgumentOutOfRangeException> (() => new AudioSource ("voice", 1, 1, 1, 64000, 100000, 512, 11, false));
		}

		[Test]
		public void Values()
		{
			var source = new AudioSource ("voice", 1, 1, 1, 64000, 44100, 512, 10, true);
			Assert.AreEqual ("voice",	source.Name);
			Assert.AreEqual (1,			source.Id);
			Assert.AreEqual (1,			source.OwnerId);
			Assert.AreEqual (1,			source.Channels);
			Assert.AreEqual (64000,		source.Bitrate);
			Assert.AreEqual (44100,		source.Frequency);
			Assert.AreEqual (512,		source.FrameSize);
			Assert.AreEqual (10,		source.Complexity);
			Assert.AreEqual (true,		source.IsMuted);
		}

		[Test]
		public void SerializeDeserialize()
		{
			var stream = new MemoryStream (new byte[20480], true);
			var writer = new StreamValueWriter (stream);
			var reader = new StreamValueReader (stream);

			var source = new AudioSource ("voice", 1, 1, 1, 64000, 44100, 512, 10, true);
			source.Serialize (writer);
			long length = stream.Position;
			stream.Position = 0;

			source = new AudioSource (reader);
			Assert.AreEqual (length,	stream.Position);
			Assert.AreEqual ("voice",	source.Name);
			Assert.AreEqual (1,			source.Id);
			Assert.AreEqual (1,			source.OwnerId);
			Assert.AreEqual (1,			source.Channels);
			Assert.AreEqual (64000,		source.Bitrate);
			Assert.AreEqual (44100,		source.Frequency);
			Assert.AreEqual (512,		source.FrameSize);
			Assert.AreEqual (10,		source.Complexity);
			Assert.AreEqual (true,		source.IsMuted);
		}
	}
}