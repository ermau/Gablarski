// Copyright (c) 2009, Eric Maupin
// All rights reserved.
//
// Redistribution and use in source and binary forms, with
// or without modification, are permitted provided that
// the following conditions are met:
//
// - Redistributions of source code must retain the above 
//   copyright notice, this list of conditions and the
//   following disclaimer.
//
// - Redistributions in binary form must reproduce the above
//   copyright notice, this list of conditions and the
//   following disclaimer in the documentation and/or other
//   materials provided with the distribution.
//
// - Neither the name of Gablarski nor the names of its
//   contributors may be used to endorse or promote products
//   or services derived from this software without specific
//   prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS
// AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED
// WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
// WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR
// PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT
// HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT,
// INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
// (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE
// GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
// INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY,
// WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
// NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
// THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH
// DAMAGE.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Gablarski.OpenAL
{
	public class SourcePool<T>
		: IDisposable
		where T : class
	{
		public event EventHandler<SourceFinishedEventArgs<T>> SourceFinished;

		public Source RequestSource (T owner)
		{
			OpenAL.DebugFormat ("SourcePool: Requesting source for {0}", owner);

			Source free;

			lock (owners)
			{
				if (owner == lastOwner && lastSource != null)
				{
					OpenAL.DebugFormat ("SourcePool: Returning last source for {0}", owner);

					return lastSource;
				}

				free = owners.Where (kvp => kvp.Value == owner).Select (kvp => kvp.Key).FirstOrDefault();
	
				if (free == null)
				{
					free = owners.Where (kvp => kvp.Value == null).Select (kvp => kvp.Key).FirstOrDefault ();
	
					if (free == null)
					{
						free = Source.Generate ();
						OpenAL.DebugFormat ("SourcePool: Couldn't find a free source for {0}, created {1}", owner, free);
					}
					else
						OpenAL.DebugFormat ("SourcePool: Found free source {0} for {1}", free, owner);
	
					owners[free] = owner;
				}
				else
					OpenAL.DebugFormat ("SourcePool: Found owned source {0} for {1}", free, owner);

				lastOwner = owner;
				lastSource = free;
			}
			
			return free;
		}

		public void FreeSource (T sourceOwner)
		{
			lock (owners)
			{
				var source = owners.FirstOrDefault (kvp => kvp.Value == sourceOwner).Key;
				if (source == null)
					return;

				owners[source] = default (T);

				if (source == lastSource)
					lastSource = null;
			}
		}

		public void FreeSource (Source source)
		{
			lock (owners)
			{
				owners[source] = default(T);

				if (source == lastSource)
					lastSource = null;
			}
		}

		public void FreeSources (IEnumerable<Source> sources)
		{
			lock (owners)
			{
				foreach (Source csource in sources)
				{
					owners[csource] = default (T);

					if (csource == lastSource)
						lastSource = null;
				}
			}
		}

		public void Tick()
		{
			List<KeyValuePair<Source, T>> finished;
			lock (owners)
				finished = owners.Where (kvp => kvp.Key.IsStopped && kvp.Value != null).ToList();

			for (int i = 0; i < finished.Count; ++i)
			{
				var s = finished[i].Key;
				var o = finished[i].Value;

				OpenAL.DebugFormat ("SourcePool: {0} is stopped, freeing", s);
				FreeSource (s);
				OnSourceFinished (new SourceFinishedEventArgs<T> (o, s));
			}
		}

		private readonly Dictionary<Source, T> owners = new Dictionary<Source, T> ();
		private T lastOwner;
		private Source lastSource;

		private void OnSourceFinished (SourceFinishedEventArgs<T> e)
		{
			var finished = this.SourceFinished;
			if (finished != null)
				finished (this, e);
		}

		public void Dispose()
		{
			lock (owners)
			{
				foreach (Source s in owners.Keys)
					s.Dispose();

				owners.Clear();
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