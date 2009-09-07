// Copyright (c) 2009, Eric Maupin
// All rights reserved.

// Redistribution and use in source and binary forms, with
// or without modification, are permitted provided that
// the following conditions are met:

// - Redistributions of source code must retain the above 
//   copyright notice, this list of conditions and the
//   following disclaimer.

// - Redistributions in binary form must reproduce the above
//   copyright notice, this list of conditions and the
//   following disclaimer in the documentation and/or other
//   materials provided with the distribution.

// - Neither the name of Gablarski nor the names of its
//   contributors may be used to endorse or promote products
//   derived from this software without specific prior
//   written permission.

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