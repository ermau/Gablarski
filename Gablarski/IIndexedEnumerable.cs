using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gablarski
{
	public interface IIndexedEnumerable<TKey, TValue>
		: IEnumerable<TValue>
	{
		TValue this[TKey key] { get; }
	}
}
