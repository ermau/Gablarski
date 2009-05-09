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
		public SourcePool ()
		{
			this.collecting = true;
			Thread collector = new Thread (this.Collector);
			collector.IsBackground = true;
			collector.Start ();
		}

		public Source RequestSource (T owner)
		{
			rwl.EnterUpgradeableReadLock ();

			Source free = null;
			foreach (var kvp in owners)
			{
				if (kvp.Value == null)
				{
					free = kvp.Key;
					break;
				}
				else if (kvp.Value == owner)
				{
					rwl.ExitUpgradeableReadLock ();
					return kvp.Key;
				}
			}

			if (free == null)
				free = Source.Generate ();

			rwl.EnterWriteLock ();
			owners[free] = owner;
			rwl.ExitWriteLock ();
			rwl.ExitUpgradeableReadLock ();

			return free;
		}

		public void FreeSource (Source source)
		{
			rwl.EnterWriteLock ();
			owners[source] = default(T);
			rwl.ExitWriteLock ();
		}

		public void FreeSources (IEnumerable<Source> sources)
		{
			rwl.EnterWriteLock ();

			foreach (Source csource in sources)
				owners[csource] = default (T);

			rwl.ExitWriteLock ();
		}

		private readonly Dictionary<Source, T> owners = new Dictionary<Source, T> ();
		
		private readonly ReaderWriterLockSlim rwl = new ReaderWriterLockSlim ();
		private volatile bool collecting;

		private void Collector ()
		{
			while (this.collecting)
			{
				List<Source> toFree = new List<Source> (owners.Count);

				rwl.EnterReadLock ();
				foreach (Source source in owners.Keys)
				{
					if (source.ProcessedBuffers > 0)
					{
						SourceBuffer[] buffers = source.Dequeue ();
						for (int i = 0; i < buffers.Length; ++i)
							buffers[i].Dispose ();
					}

					if (!source.IsPlaying)
						toFree.Add (source);
				}
				rwl.ExitReadLock ();

				if (toFree.Count > 0)
					this.FreeSources (toFree);

				Thread.Sleep (1);
			}
		}
	}
}