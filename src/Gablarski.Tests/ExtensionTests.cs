using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cadenza;
using NUnit.Framework;

namespace Gablarski.Tests
{
	[TestFixture]
	public class ExtensionTests
	{
		[Test]
		public void IsEmpty()
		{
			string foo = null;
			Assert.IsTrue (foo.IsNullOrWhitespace());

			foo = String.Empty;
			Assert.IsTrue (foo.IsNullOrWhitespace());

			foo = "   ";
			Assert.IsTrue (foo.IsNullOrWhitespace());

			foo = "wee ";
			Assert.IsFalse (foo.IsNullOrWhitespace());
		}

		[Test]
		public void Trim()
		{
			int bar = 2;
			Assert.AreEqual (1, bar.Trim (1));
			Assert.AreEqual (2, bar.Trim (2));
			Assert.AreEqual (2, bar.Trim (3));
		}
	}
}