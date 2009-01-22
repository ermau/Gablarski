using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gablarski
{
	public static class IEnumerableExtensions
	{
		public static TValue MaxOrDefault<TElement, TValue> (this IEnumerable<TElement> self, Func<TElement, TValue> selector, TValue def)
		{
			return (self.Any()) ? self.Max (selector) : def;
		}
	}
}