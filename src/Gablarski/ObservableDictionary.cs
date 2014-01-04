//
// ObservableDictionary.cs
//
// Author:
//   Eric Maupin <me@ermau.com>
//
// Copyright (c) 2013 Eric Maupin
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace Gablarski
{
	sealed class ObservableDictionary<TKey, TValue>
		: IDictionary<TKey, TValue>, INotifyCollectionChanged
	{
		public ObservableDictionary()
		{
			this.dict = new Dictionary<TKey, TValue>();
			this.keys = new ReadOnlyObservableCollection<TKey> (this.dict.Keys);
			this.values = new ReadOnlyObservableCollection<TValue> (this.dict.Values);
		}

		public ObservableDictionary (int capacity)
		{
			this.dict = new Dictionary<TKey, TValue> (capacity);
			this.keys = new ReadOnlyObservableCollection<TKey>(this.dict.Keys);
			this.values = new ReadOnlyObservableCollection<TValue>(this.dict.Values);
		}

		public event NotifyCollectionChangedEventHandler CollectionChanged;

		public int Count
		{
			get { return this.dict.Count; }
		}

		public TValue this[TKey key]
		{
			get { return this.dict[key]; }
			set
			{
				bool changed = this.dict.ContainsKey (key);
				this.dict[key] = value;

				var action = (changed) ? NotifyCollectionChangedAction.Replace : NotifyCollectionChangedAction.Add;
				Signal (action, key, value, changed);
			}
		}

		public ICollection<TKey> Keys
		{
			get { return this.keys; }
		}

		public ICollection<TValue> Values
		{
			get { return this.values; }
		}

		bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly
		{
			get { return false; }
		}

		void ICollection<KeyValuePair<TKey, TValue>>.Add (KeyValuePair<TKey, TValue> item)
		{
			Add (item.Key, item.Value);
		}

		public void Clear()
		{
			this.dict.Clear();
			Signal (NotifyCollectionChangedAction.Reset);
		}

		bool ICollection<KeyValuePair<TKey, TValue>>.Contains (KeyValuePair<TKey, TValue> item)
		{
			return ((ICollection<KeyValuePair<TKey, TValue>>)this.dict).Contains (item);
		}

		void ICollection<KeyValuePair<TKey, TValue>>.CopyTo (KeyValuePair<TKey, TValue>[] array, int arrayIndex)
		{
			((ICollection<KeyValuePair<TKey, TValue>>)this.dict).CopyTo (array, arrayIndex);
		}

		bool ICollection<KeyValuePair<TKey, TValue>>.Remove (KeyValuePair<TKey, TValue> item)
		{
			bool removed = ((ICollection<KeyValuePair<TKey, TValue>>)this.dict).Remove (item);
			if (removed) {
				Signal (NotifyCollectionChangedAction.Remove, item.Key, item.Value);
			}

			return removed;
		}

		public bool ContainsKey (TKey key)
		{
			return this.dict.ContainsKey (key);
		}

		public void Add (TKey key, TValue value)
		{
			this.dict.Add (key, value);

			Signal (NotifyCollectionChangedAction.Add, key, value);
		}

		public bool Remove (TKey key)
		{
			TValue value;
			bool present = this.dict.TryGetValue (key, out value);
			if (present) {
				this.dict.Remove (key);

				Signal (NotifyCollectionChangedAction.Remove, key, value);
			}

			return present;
		}

		public bool TryGetValue (TKey key, out TValue value)
		{
			return this.dict.TryGetValue (key, out value);
		}

		public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
		{
			return this.dict.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		private readonly ReadOnlyObservableCollection<TKey> keys;
		private readonly ReadOnlyObservableCollection<TValue> values;
		private readonly Dictionary<TKey, TValue> dict;

		private void OnCollectionChanged (NotifyCollectionChangedEventArgs e)
		{
			NotifyCollectionChangedEventHandler handler = this.CollectionChanged;
			if (handler != null)
				handler (this, e);
		}

		private void Signal (NotifyCollectionChangedAction action)
		{
			var e = new NotifyCollectionChangedEventArgs (action);
			OnCollectionChanged (e);

			this.keys.SignalCollectionChanged (this, e);
			this.values.SignalCollectionChanged (this, e);
		}

		private void Signal (NotifyCollectionChangedAction action, TKey key, TValue value, bool valueOnly = false)
		{
			OnCollectionChanged (new NotifyCollectionChangedEventArgs (action, new KeyValuePair<TKey, TValue> (key, value), 0));

			if (!valueOnly)
				this.keys.SignalCollectionChanged (this, new NotifyCollectionChangedEventArgs (action, key, 0));

			this.values.SignalCollectionChanged (this, new NotifyCollectionChangedEventArgs (action, value, 0));
		}

		private sealed class ReadOnlyObservableCollection<T>
			: ICollection<T>, INotifyCollectionChanged
		{
			private readonly ICollection<T> collection;

			public ReadOnlyObservableCollection (ICollection<T> collection)
			{
				if (collection == null)
					throw new ArgumentNullException ("collection");

				this.collection = collection;
			}

			public event NotifyCollectionChangedEventHandler CollectionChanged;

			public int Count
			{
				get { return this.collection.Count; }
			}

			public bool IsReadOnly
			{
				get { return true; }
			}

			public void SignalCollectionChanged (object sender, NotifyCollectionChangedEventArgs e)
			{
				if (e == null)
					throw new ArgumentNullException ("e");

				var changed = CollectionChanged;
				if (changed != null)
					changed (sender, e);
			}

			public IEnumerator<T> GetEnumerator()
			{
				return this.collection.GetEnumerator();
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return GetEnumerator();
			}

			public void Add (T item)
			{
				throw new NotSupportedException();
			}

			public void Clear()
			{
				throw new NotSupportedException();
			}

			public bool Contains (T item)
			{
				return this.collection.Contains (item);
			}

			public void CopyTo (T[] array, int arrayIndex)
			{
				this.collection.CopyTo (array, arrayIndex);
			}

			public bool Remove (T item)
			{
				throw new NotSupportedException();
			}
		}
	}
}