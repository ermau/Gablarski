using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gablarski
{
	public static class IEnumerableExtensions
	{
		public static V MaxOrDefault<T, V> (this IEnumerable<T> self, Func<T, V> selector, V def)
		{
			return (self.Any()) ? self.Max (selector) : def;
		}
	}
}