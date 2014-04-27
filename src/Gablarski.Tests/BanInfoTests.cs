using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Gablarski.Tests
{
	[TestFixture]
	public class BanInfoTests
	{
		[Test]
		public void CtorNull()
		{
			Assert.Throws<ArgumentNullException> (() => new BanInfo (null, null, TimeSpan.Zero));
			Assert.DoesNotThrow (() => new BanInfo ("192.168.1.*", null, TimeSpan.Zero));
			Assert.DoesNotThrow (() => new BanInfo (null, "username", TimeSpan.Zero));
		}

		[Test]
		public void Ctor()
		{
			var info = new BanInfo ("192.168.1.*", null, TimeSpan.Zero);
			Assert.AreEqual ("192.168.1.*", info.IPMask);
			Assert.AreEqual (null, info.Username);
			Assert.AreEqual (DateTime.Today, info.Created.Date);
			Assert.AreEqual (TimeSpan.Zero, info.Length);

			info = new BanInfo (null, "monkeys", TimeSpan.FromHours (1.1));
			Assert.AreEqual (null, info.IPMask);
			Assert.AreEqual ("monkeys", info.Username);
			Assert.AreEqual (DateTime.Today, info.Created.Date);
			Assert.AreEqual (TimeSpan.FromHours (1.1), info.Length);
		}

		[Test]
		public void IsNeverExpired()
		{
			var info = new BanInfo ("192.168.1.*", null, TimeSpan.Zero);
			Assert.IsFalse (info.IsExpired);

			info.Created = new DateTime (1990, 1, 1);
			Assert.IsFalse (info.IsExpired);
		}

		[Test]
		public void IsExpired()
		{
			var info = new BanInfo ("192.168.1.*", null, TimeSpan.FromHours (1));
			info.Created = new DateTime (1990, 1, 1);
			Assert.IsTrue (info.IsExpired);
		}

		[Test]
		public void IsNotExpired()
		{
			var info = new BanInfo ("192.168.1.*", null, TimeSpan.FromDays (30));
			info.Created = DateTime.Today.Subtract (TimeSpan.FromDays (28));
			Assert.IsFalse (info.IsExpired);
		}
	}
}