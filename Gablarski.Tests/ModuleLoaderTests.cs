using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using NUnit.Framework;

namespace Gablarski.Tests
{
	public interface IMockContract
	{
		string Name { get; }
	}

	public class MockContractImplementation1
		: IMockContract
	{
		public string Name { get { return "Foo"; }}
	}

	public class MockContractImplementation2
		: IMockContract
	{
		public string Name { get { return "Foo"; }}
	}

	[TestFixture]
	public class ModuleLoaderTests
	{
		[Test]
		public void InvalidLoader()
		{
			Assert.Throws<ArgumentException> (() => new ModuleLoader<string>());
			Assert.Throws<ArgumentNullException> (() => new ModuleLoader<IMockContract> (ModuleLoaderOptions.SearchExecuting, null));
		}

		[Test]
		public void SearchAssembly()
		{
			var impl = ModuleLoader<IMockContract>.SearchAssembly (Assembly.GetExecutingAssembly(), typeof (IMockContract));
			Assert.IsTrue (impl.Contains (typeof(MockContractImplementation1)));
			Assert.IsTrue (impl.Contains (typeof(MockContractImplementation2)));
		}
	}
}