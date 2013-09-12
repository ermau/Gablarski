// Copyright (c) 2012-2013, Eric Maupin
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
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Gablarski.Annotations;

namespace Gablarski.Clients.ViewModels
{
	public sealed class ObservableFilter<T, TSet>
		: IEnumerable<T>, INotifyCollectionChanged
		where TSet : IEnumerable<T>, INotifyCollectionChanged
	{
		public ObservableFilter ([NotNull] TSet source, [NotNull] Func<T, bool> filter)
		{
			if (source == null)
				throw new ArgumentNullException ("source");
			if (filter == null)
				throw new ArgumentNullException ("filter");
			
			source.CollectionChanged += OnSourceCollectionChanged;

			this.source = source;
			this.filter = filter;
		}

		public event NotifyCollectionChangedEventHandler CollectionChanged;

		public IEnumerator<T> GetEnumerator()
		{
			return this.source.Where (this.filter).GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		private readonly TSet source;
		private readonly Func<T, bool> filter;

		private void OnCollectionChanged (NotifyCollectionChangedEventArgs e)
		{
			var handler = CollectionChanged;
			if (handler != null)
				handler (this, e);
		}

		private void OnSourceCollectionChanged (object sender, NotifyCollectionChangedEventArgs e)
		{
			T[] newItems = (e.NewItems != null) ? e.NewItems.OfType<T>().Where (this.filter).ToArray() : null;
			IList oldItems = e.OldItems;

			switch (e.Action)
			{
				case NotifyCollectionChangedAction.Add:
					if (newItems != null && newItems.Length > 0)
						OnCollectionChanged (new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Add, newItems));

					break;

				case NotifyCollectionChangedAction.Remove:

					OnCollectionChanged (new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Remove, e.NewItems));
					break;

				case NotifyCollectionChangedAction.Reset:
					if (newItems != null && newItems.Length > 0)
						OnCollectionChanged (new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Reset, newItems));
					else
						OnCollectionChanged (new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Reset));

					break;

				case NotifyCollectionChangedAction.Replace:
					if (newItems != null && newItems.Length > 0)
					{
						if (oldItems != null && oldItems.Count > 0)
							OnCollectionChanged (new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Replace, newItems, oldItems));
						else
							OnCollectionChanged (new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Add, newItems));
					}
					else if (oldItems != null && oldItems.Count > 0)
						OnCollectionChanged (new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Remove, oldItems));

					break;
			}
		}
	}
}