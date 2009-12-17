// Copyright (c) 2009, Eric Maupin
// All rights reserved.
//
// Redistribution and use in source and binary forms, with
// or without modification, are permitted provided that
// the following conditions are met:
//
// - Redistributions of source code must retain the above 
//   copyright notice, this list of conditions and the
//   following disclaimer.
//
// - Redistributions in binary form must reproduce the above
//   copyright notice, this list of conditions and the
//   following disclaimer in the documentation and/or other
//   materials provided with the distribution.
//
// - Neither the name of Gablarski nor the names of its
//   contributors may be used to endorse or promote products
//   or services derived from this software without specific
//   prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS
// AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED
// WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
// WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR
// PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT
// HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT,
// INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
// (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE
// GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
// INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY,
// WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
// NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
// THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH
// DAMAGE.

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