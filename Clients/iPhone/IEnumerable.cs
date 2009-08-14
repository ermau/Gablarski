using System;
using System.Collections.Generic;
using Mono.Rocks;

namespace Gablarski.Clients.iPhone
{
	public static class Enumerable
	{
		public static IEnumerable<T> Where<T> (this IEnumerable<T> self, Func<T, bool> predicate)
		{
			Check.Self (self);
			Check.Predicate (predicate);
			
			foreach (T item in self)
			{
				if (predicate (item))
					yield return item;
			}
		}
		
		public static T First<T> (this IEnumerable<T> self)
		{
			Check.Self (self);
			
			using (var en = self.GetEnumerator())
			{
				if (!en.MoveNext())
					throw new InvalidOperationException();
			
				return en.Current;
			}
		}
		
		public static T First<T> (this IEnumerable<T> self, Func<T, bool> predicate)
		{
			Check.Self (self);
			Check.Predicate (predicate);
			
			foreach (T item in self)
			{
				if (predicate (item))
					return item;
			}
			
			throw new InvalidOperationException();
		}
		
		public static T FirstOrDefault<T> (this IEnumerable<T> self)
		{
			Check.Self (self);
			
			using (var en = self.GetEnumerator())
			{
				if (!en.MoveNext())
					return default(T);
				
				return en.Current;
			}
		}
		
		public static T FirstOrDefault<T> (this IEnumerable<T> self, Func<T, bool> predicate)
		{
			Check.Self (self);
			Check.Predicate (predicate);
			
			foreach (T item in self)
			{
				if (predicate (item))
					return item;
			}
			
			return default(T);
		}
		
		public static int Count<T> (this IEnumerable<T> self)
		{
			var list = (self as ICollection<T>);
			if (list != null)
				return list.Count;
			else
			{
				int c = 0;
				foreach (T item in self)
					++c;
				
				return c;
			}
		}
	}
}