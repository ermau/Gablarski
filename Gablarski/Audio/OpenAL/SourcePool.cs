using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Gablarski.Audio.OpenAL
{
	public class SourcePool<T>
		where T : class
	{
		public SourcePool()
		{
			this.sources = new Source[Source.MaxSources];
			this.owners = new T[Source.MaxSources];
		}

		public Source RequestSource (T owner)
		{
			var s = this.sources;
			var o = this.owners;

			int lastFree = 0;
			for (int i = 0; i < this.sources.Length; ++i)
			{
				if (o[i] == owner)
					return sources[i];

				if (o[i] == default(T))
					lastFree = i;
			}

			if (s[lastFree] == null)
				s[lastFree] = Source.Generate ();

			return s[lastFree];
		}

		public void FreeSource (Source source)
		{
			for (int i = 0; i < owners.Length; ++i)
			{
				if (sources[i] != source)
					continue;

				owners[i] = default(T);
			}
		}

		public void FreeSources (IEnumerable<Source> freeSources)
		{
			for (int i = 0; i < sources.Length; ++i)
			{
				if (!freeSources.Contains (sources[i]))
					owners[i] = default(T);
			}
		}

		private readonly T[] owners;
		private readonly Source[] sources;
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