using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gablarski.Audio;
using NUnit.Framework;

namespace Gablarski.Tests
{
	[TestFixture]
	public class AudioFormatTests
	{
		[Test]
		public void Equals()
		{
			var one = new AudioFormat (WaveFormatEncoding.LPCM, 2, 16, 48000);
			var two = new AudioFormat (WaveFormatEncoding.LPCM, 2, 16, 48000);

			Assert.IsTrue (one.Equals (two));
		}

		[Test]
		public void DoesNotEqual()
		{
			var one = new AudioFormat (WaveFormatEncoding.Unknown, 2, 16, 48000);
			var two = new AudioFormat (WaveFormatEncoding.LPCM, 2, 16, 48000);
			Assert.IsFalse (one.Equals (two));

			one = new AudioFormat (WaveFormatEncoding.LPCM, 1, 16, 48000);
			Assert.IsFalse (one.Equals (two));

			one = new AudioFormat (WaveFormatEncoding.LPCM, 2, 8, 48000);
			Assert.IsFalse (one.Equals (two));

			one = new AudioFormat (WaveFormatEncoding.LPCM, 2, 16, 44100);
			Assert.IsFalse (one.Equals (two));
		}

		[Test]
		public void GetHashCodeTest()
		{
			var one = new AudioFormat (WaveFormatEncoding.LPCM, 2, 16, 48000);
			var two = new AudioFormat (WaveFormatEncoding.LPCM, 2, 16, 48000);

			Assert.AreEqual (one.GetHashCode(), two.GetHashCode());
		}

		[Test]
		public void GetHashCodeNotMatching()
		{
			var one = new AudioFormat (WaveFormatEncoding.Unknown, 2, 16, 48000);
			var two = new AudioFormat (WaveFormatEncoding.LPCM, 2, 16, 48000);
			Assert.AreNotEqual (one.GetHashCode(), two.GetHashCode());

			one = new AudioFormat (WaveFormatEncoding.LPCM, 1, 16, 48000);
			Assert.AreNotEqual (one.GetHashCode(), two.GetHashCode());

			one = new AudioFormat (WaveFormatEncoding.LPCM, 2, 8, 48000);
			Assert.AreNotEqual (one.GetHashCode(), two.GetHashCode());

			one = new AudioFormat (WaveFormatEncoding.LPCM, 2, 16, 44100);
			Assert.AreNotEqual (one.GetHashCode(), two.GetHashCode());
		}
	}
}