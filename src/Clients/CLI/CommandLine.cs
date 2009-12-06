using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Mono.Options;
using Cadenza;

namespace Gablarski.Clients.CLI
{
	public static class CommandLine
	{
		public static IList<string> Parse (this OptionSet self, string args)
		{
			if (self == null)
				throw new ArgumentNullException("self");
			if (args == null)
				throw new ArgumentNullException("args");

			return self.Parse (Parse (args));
		}

		public static IList<string> Parse (string args)
		{
			if (args == null)
				throw new ArgumentNullException ("args");

			List<string> arglist = new List<string>();

			int lastSpace = 0;
			bool inQuotes = false;
			int i = 0;
			for (; i < args.Length; ++i)
			{		
				switch (args[i])
				{
					case '"':
						//if (i > 0 && args[i-1] == '\\') // Escaped
						//    break;

						inQuotes = !inQuotes;
						break;

					case ' ':
						if (inQuotes)
							break;

						string arg = args.Substring (lastSpace, i - lastSpace);
						if (arg.StartsWith ("\"") && arg.EndsWith ("\""))
							arg = arg.Substring (1, arg.Length - 2);

						arglist.Add (arg);
						lastSpace = i + 1;
						break;
				}
			}

			if (lastSpace != args.Length)
			{
				string arg = args.Substring (lastSpace, args.Length - lastSpace);
				if (arg.StartsWith ("\"") && arg.EndsWith ("\""))
					arg = arg.Substring (1, arg.Length - 2);

				arglist.Add (arg);
			}

			return new ReadOnlyCollection<string> (arglist);
		}
	}
}