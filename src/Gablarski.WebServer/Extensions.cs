using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using HttpServer;

namespace Gablarski.WebServer
{
	public static class Extensions
	{
		public static IEnumerable<T> RunQuery<T> (this IEnumerable<T> self, IHttpInput query)
		{
			if (self == null)
				throw new ArgumentNullException ("self");
			if (query == null)
				throw new ArgumentNullException ("query");

			foreach (HttpInputItem input in query.Where (i => i.Value != null))
			{
				switch (input.Name)
				{
					case "$skip":
					{
						int skip;
						if (Int32.TryParse (input.Value, out skip))
							self = self.Skip (skip);

						break;
					}

					case "$top":
					{
						int top;
						if (Int32.TryParse (input.Value, out top))
							self = self.Take (top);

						break;
					}
				}
			}
			
			return self;
		}
		
		public static bool TryGetItemId (this IHttpRequest request, out int itemId)
		{
			string part = request.UriParts[0];

			int arg = part.IndexOf ("(") + 1;
			if (arg != 0 && part[part.Length - 1] == ')')
				part = part.Substring (arg, part.Length - 1 - arg);

			if (!Int32.TryParse (part, out itemId))
				return false;
			
			return true;
		}

		public static bool ContainsAndNotNull (this IHttpInput self, params string[] fieldNames)
		{
			return fieldNames.All (n => self.Contains (n) && self[n].Value != null);
		}
	}
}