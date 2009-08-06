using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gablarski.Media.Sources;
using NUnit.Framework;

namespace Gablarski.Tests
{
	[TestFixture]
	public class AudioSourceTests
	{
		[Test]
		public void InvalidSource()
		{
			Assert.Throws<ArgumentNullException> (() => new AudioSource (null, 1, 1, 1, 64000, 44100, 512, 10));

			Assert.Throws<ArgumentOutOfRangeException> (() => new AudioSource ("voice", 0, 1, 1, 64000, 44100, 512, 10));

			Assert.Throws<ArgumentOutOfRangeException> (() => new AudioSource ("voice", 1, -1, 1, 64000, 44100, 512, 10));

			Assert.Throws<ArgumentOutOfRangeException> (() => new AudioSource ("voice", 1, 1, 0, 0, 41000, 512, 10));
			Assert.Throws<ArgumentOutOfRangeException> (() => new AudioSource ("voice", 1, 1, 3, 0, 41000, 512, 10));

			Assert.Throws<ArgumentOutOfRangeException> (() => new AudioSource ("voice", 1, 1, 1, 0, 41000, 512, 10));

			Assert.Throws<ArgumentOutOfRangeException> (() => new AudioSource ("voice", 1, 1, 1, 64000, 0, 512, 10));
			Assert.Throws<ArgumentOutOfRangeException> (() => new AudioSource ("voice", 1, 1, 1, 64000, 100000, 512, 10));

			Assert.Throws<ArgumentOutOfRangeException> (() => new AudioSource ("voice", 1, 1, 1, 64000, 44100, 32, 10));
			Assert.Throws<ArgumentOutOfRangeException> (() => new AudioSource ("voice", 1, 1, 1, 64000, 44100, 1024, 10));

			Assert.Throws<ArgumentOutOfRangeException> (() => new AudioSource ("voice", 1, 1, 1, 64000, 100000, 512, 0));
			Assert.Throws<ArgumentOutOfRangeException> (() => new AudioSource ("voice", 1, 1, 1, 64000, 100000, 512, 11));
		}

		[Test]
		public void Values()
		{
			int user = 1;
			var source = new AudioSource ("voice", 1, user, 1, 64000, 44100, 512, 10);
			Assert.AreEqual ("voice",	source.Name);
			Assert.AreEqual (1,			source.Id);
			Assert.AreEqual (user,		source.OwnerId);
			Assert.AreEqual (1,			source.Channels);
			Assert.AreEqual (64000,	 source.Bitrate);
			Assert.AreEqual (44100,	 source.Frequency);
			Assert.AreEqual (512,		source.FrameSize);
			Assert.AreEqual (10,		source.Complexity);
		}
	}
}