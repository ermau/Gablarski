using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gablarski.Clients.CLI;
using Mono.Options;
using NUnit.Framework;

namespace Gablarski.Tests
{
	[TestFixture]
	public class OptionParseExtensionTests
	{
		[Test]
		public void InvalidParse()
		{
			Assert.Throws<ArgumentNullException> (() => OptionsExtensions.Parse (null, "Foo"));
			Assert.Throws<ArgumentNullException> (() => OptionsExtensions.Parse (new OptionSet(), null));
		}

		[Test]
		public void ParseCoreBasic ()
		{
			string args = "-foo=\"bar\" -bar=\"foo\"";

			var parsedArgs = OptionsExtensions.ParseCore (args);

			Assert.AreEqual (1, (parsedArgs.Count (s => s == "-foo=\"bar\"")));
			Assert.AreEqual (1, (parsedArgs.Count (s => s == "-bar=\"foo\"")));
		}

		//[Test]
		//public void ParseCoreEscaped()
		//{
		//    string args = @"-foo=\"" -bar=\""w ee"" -v";

		//    var parsedArgs = OptionsExtensions.ParseCore (args);

		//    Assert.AreEqual (1, (parsedArgs.Count (s => s == "-foo=\"")));
		//    Assert.AreEqual (1, (parsedArgs.Count (s => s == "-bar=\"w")));
		//    Assert.AreEqual (1, (parsedArgs.Count (s => s == "ee\"")));
		//    Assert.AreEqual (1, (parsedArgs.Count (s => s == "-v")));
		//}

		[Test]
		public void ParseCore()
		{
			string args = "-foo=\"bar foo\" monkeys -d=foo 1";

			var parsedArgs = OptionsExtensions.ParseCore (args);

			Assert.AreEqual (1, parsedArgs.Count (s => s == "-foo=\"bar foo\""));
			Assert.AreEqual (1, parsedArgs.Count (s => s == "monkeys"));
			Assert.AreEqual (1, parsedArgs.Count (s => s == "-d=foo"));
			Assert.AreEqual (1, parsedArgs.Count (s => s == "1"));
		}
	}
}