// Copyright (c) 2014, Eric Maupin
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
using Cadenza.Collections;

namespace Gablarski
{
	sealed class ObservableLookup<TKey, TElement>
		: IMutableLookup<TKey, TElement>, INotifyCollectionChanged, IList
	{
		public ObservableLookup()
		{
		}

		public ObservableLookup (IEnumerable<IGrouping<TKey, TElement>> lookup)
		{
			if (lookup == null)
				throw new ArgumentNullException ("lookup");

			foreach (var group in lookup)
				Add (group.Key, group);
		}

		public event NotifyCollectionChangedEventHandler CollectionChanged;

		public int Count
		{
			get { return this.groupings.Count; }
		}

		public IEnumerable<TElement> this [TKey key]
		{
			get { return this.groupings[key]; }
		}

		public bool Contains (TKey key)
		{
			return this.groupings.ContainsKey (key);
		}

		public void Add (TKey key, TElement element)
		{
			Grouping grouping;
			if (!this.groupings.TryGetValue (key, out grouping))
				grouping = new Grouping (key);

			grouping.Add (element);
		}

		public void Add (TKey key, IEnumerable<TElement> elements)
		{
			Grouping grouping;
			if (!this.groupings.TryGetValue (key, out grouping)) {
				grouping = new Grouping (key);

				int index = this.groupings.Count;
				this.groupings.Add (key, grouping);
				OnCollectionChanged (new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Add, grouping, index));
			}

			grouping.AddRange (elements);
		}

		public bool Remove (TKey key, TElement element)
		{
			Grouping grouping;
			if (!this.groupings.TryGetValue (key, out grouping))
				return false;

			if (grouping.Remove (element)) {
				if (grouping.Count == 0) {
					int index = this.groupings.IndexOf (key);
					this.groupings.RemoveAt (index);
					OnCollectionChanged (new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Remove, grouping, index));
				}

				return true;
			}

			return false;
		}

		public bool Remove (TKey key)
		{
			Grouping grouping;
			if (!this.groupings.TryGetValue (key, out grouping))
				return false;

			this.groupings.Remove (key);
			OnCollectionChanged (new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Remove, grouping));
			return true;
		}

		public void Clear()
		{
			this.groupings.Clear();
			OnCollectionChanged (new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Reset));
		}

		public IEnumerator<IGrouping<TKey, TElement>> GetEnumerator()
		{
			return this.groupings.Values.Cast<IGrouping<TKey, TElement>>().GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		private readonly OrderedDictionary<TKey, Grouping> groupings = new OrderedDictionary<TKey, Grouping>();

		class Grouping
			: ObservableList<TElement>, IGrouping<TKey, TElement>
		{
			public Grouping (TKey key)
			{
				Key = key;
			}

			public TKey Key
			{
				get;
				private set;
			}
		}

		private void OnCollectionChanged (NotifyCollectionChangedEventArgs e)
		{
			NotifyCollectionChangedEventHandler handler = this.CollectionChanged;
			if (handler != null)
				handler (this, e);
		}

		object IList.this [int index]
		{
			get { return this.groupings[index]; }
			set { throw new NotSupportedException(); }
		}

		bool IList.IsReadOnly
		{
			get { return true; }
		}

		bool IList.IsFixedSize
		{
			get { return false; }
		}

		object ICollection.SyncRoot
		{
			get { return this; }
		}

		bool ICollection.IsSynchronized
		{
			get { return false; }
		}

		int IList.Add (object value)
		{
			throw new NotSupportedException();
		}

		bool IList.Contains (object value)
		{
			throw new NotSupportedException();
		}

		void ICollection.CopyTo (Array array, int index)
		{
			throw new NotImplementedException();
		}

		int IList.IndexOf (object value)
		{
			if (!(value is TKey))
				throw new ArgumentException();

			return this.groupings.IndexOf ((TKey) value);
		}

		void IList.Insert (int index, object value)
		{
			throw new NotSupportedException();
		}

		void IList.Remove (object value)
		{
			throw new NotSupportedException();
		}

		void IList.RemoveAt (int index)
		{
			throw new NotSupportedException();
		}
	}
}
