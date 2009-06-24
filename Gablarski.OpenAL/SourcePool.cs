using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Gablarski.OpenAL
{
	public class SourcePool<T>
		where T : class
	{
		public Source RequestSource (T owner)
		{
			Source free = null;
			lock (this.sourceLock)
			{
				foreach (var kvp in owners)
				{
					if (kvp.Value == null)
					{
						free = kvp.Key;
						break;
					}
					else if (kvp.Value == owner)
						return kvp.Key;
				}
			}
				
			if (free == null)
				free = Source.Generate ();

			lock (this.sourceLock)
			{
				owners[free] = owner;
			}
			
			return free;
		}

		public void FreeSource (Source source)
		{
			lock (this.sourceLock)
			{
				owners[source] = default(T);
			}
		}

		public void FreeSources (IEnumerable<Source> sources)
		{
			lock (this.sourceLock)
			{
				foreach (Source csource in sources)
					owners[csource] = default (T);
			}
		}

		private readonly Dictionary<Source, T> owners = new Dictionary<Source, T> ();
		
		private readonly object sourceLock = new object();
	}
}