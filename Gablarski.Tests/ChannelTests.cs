using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Gablarski.Tests
{
	[TestFixture]
	public class ChannelTests
	{
		[Test]
		public void IsDefault()
		{
			var idtypes = new IdentifyingTypes (typeof (Int32), typeof (Int32));
			Assert.IsTrue (Channel.IsDefault (default (Int32), idtypes));
			Assert.IsFalse (Channel.IsDefault (1, idtypes));
		}
	}
}