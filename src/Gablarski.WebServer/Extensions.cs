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
						if (query["$skip"].Value != null && Int32.TryParse (query["$skip"].Value, out skip))
							self = self.Skip (skip);

						break;
					}

					case "$top":
					{
						int top;
						if (query["$top"].Value != null && Int32.TryParse(query["$top"].Value, out top))
							self = self.Take (top);

						break;
					}

					/*case "orderby":
					{
						Type ty = typeof (T);
						PropertyInfo property = null;
						try
						{
							property = ty.GetProperty (input.Value.Trim().ToLower(), BindingFlags.Public | BindingFlags.GetProperty);

							if (property != null)
							{
								
							}
						}
						catch (AmbiguousMatchException)
						{
						}

						break;
					}*/
				}
			}
			
			return self;
		}

		public static bool Contains (this IHttpInput self, params string[] fieldNames)
		{
			return fieldNames.All (self.Contains);
		}
	}
}