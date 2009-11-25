using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Options;
using Cadenza;

namespace Gablarski.Clients.CLI
{
	public static class OptionsExtensions
	{
		public static List<string> Parse (this OptionSet self, string args)
		{
			if (self == null)
				throw new ArgumentNullException("self");
			if (args == null)
				throw new ArgumentNullException("args");

			return self.Parse (ParseCore (args));
		}

		internal static IEnumerable<string> ParseCore (string args)
		{
			if (args == null)
				throw new ArgumentNullException ("args");

			List<string> arglist = new List<string>();

			int lastSpace = 0;
			bool inQuotes = false;
			for (int i = 0; i < args.Length; ++i)
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
						
						arglist.Add (args.Substring (lastSpace, i - lastSpace));
						lastSpace = i + 1;
						break;
				}
			}

			if (lastSpace != args.Length)
				arglist.Add (args.Substring (lastSpace, args.Length - lastSpace));

			return arglist;
		}
	}
}