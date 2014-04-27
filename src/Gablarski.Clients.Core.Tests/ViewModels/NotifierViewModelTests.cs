using System;
using Gablarski.Clients.ViewModels;
using NUnit.Framework;

namespace Gablarski.Clients.Core.Tests
{
	[TestFixture]
	public class NotifierViewModelTests
	{
		[Test]
		public void CtorNull()
		{
			Assert.That (() => new NotifierViewModel (null, true), Throws.InstanceOf<ArgumentNullException>());
		}
	}
}