﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Gablarski.OpenAL
{
	public class SourcePool<T>
		where T : class
	{
		public SourcePool()
		{
			this.sourcePollerThread = new Thread (this.SourcePoller);
			this.sourcePollerThread.IsBackground = true;
			this.sourcePollerThread.Start();
		}

		public event EventHandler<SourceFinishedEventArgs<T>> SourceFinished;

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
					
					if (kvp.Value == owner)
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
		private readonly Thread sourcePollerThread;
		private volatile bool listening = true;

		private void OnSourceFinished (SourceFinishedEventArgs<T> e)
		{
			var finished = this.SourceFinished;
			if (finished != null)
				finished (this, e);
		}

		private void SourcePoller()
		{
			while (this.listening)
			{
				lock (sourceLock)
				{
					foreach (Source s in owners.Keys)
					{
						if (s.IsStopped)
							OnSourceFinished (new SourceFinishedEventArgs<T> (owners[s], s));
					}
				}

				Thread.Sleep (1);
			}
		}
	}

	public class SourceFinishedEventArgs<T>
		: EventArgs
		where T : class 
	{
		public SourceFinishedEventArgs (T owner, Source source)
		{
			this.Owner = owner;
			this.Source = source;
		}

		public T Owner
		{
			get; private set;
		}

		public Source Source
		{
			get; private set;
		}
	}
}