//
// ObservableDictionary.cs
//
// Author:
//   Eric Maupin <me@ermau.com>
//
// Copyright (c) 2013-2014 Eric Maupin
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
	public interface IReadOnlyOrderedDictionary<TKey, TValue>
		: IReadOnlyDictionary<TKey, TValue>
	{
		KeyValuePair<TKey, TValue> GetAt (int index);
	}

	public interface IOrderedDictionary<TKey, TValue>
		: IReadOnlyOrderedDictionary<TKey, TValue>, IDictionary<TKey, TValue>
	{
		void Insert (int index, TKey key, TValue value);
		void RemoveAt (int index);
	}

	sealed class ObservableDictionary<TKey, TValue>
		: IOrderedDictionary<TKey, TValue>, INotifyCollectionChanged
	{
		public ObservableDictionary()
		{
			this.keyOrder = new List<TKey>();
			this.dict = new Dictionary<TKey, TValue>();
			this.keys = new ReadOnlyObservableCollection<TKey> (this.dict.Keys);
			this.values = new ReadOnlyObservableCollection<TValue> (this.dict.Values);
		}

		public ObservableDictionary (int capacity)
		{
			if (capacity <= 0)
				throw new ArgumentOutOfRangeException ("capacity");

			this.keyOrder = new List<TKey> (capacity);
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
			set { Replace (key, value); }
		}

		public IEnumerable<TKey> Keys
		{
			get { return this.keys; }
		}

		public IEnumerable<TValue> Values
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

		public KeyValuePair<TKey, TValue> GetAt (int index)
		{
			TKey key = this.keyOrder[index];
			return new KeyValuePair<TKey, TValue> (key, this.dict[key]);
		}

		public void Clear()
		{
			this.dict.Clear();

			var args = new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Reset);
			OnCollectionChanged (args);
			this.keys.SignalCollectionChanged (args);
			this.values.SignalCollectionChanged (args);
		}

		bool ICollection<KeyValuePair<TKey, TValue>>.Contains (KeyValuePair<TKey, TValue> item)
		{
			return ((ICollection<KeyValuePair<TKey, TValue>>)this.dict).Contains (item);
		}

		public bool ContainsKey (TKey key)
		{
			return this.dict.ContainsKey (key);
		}

		public void Add (TKey key, TValue value)
		{
			int index = this.keyOrder.Count;

			this.dict.Add (key, value);
			this.keyOrder.Add (key);

			OnCollectionChanged (new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Add, new KeyValuePair<TKey, TValue> (key, value), index));
			this.keys.SignalCollectionChanged (new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Add, key, index));
			this.values.SignalCollectionChanged(new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Add, value, index));
		}

		public void Insert (int index, TKey key, TValue value)
		{
			this.dict.Add (key, value);
			this.keyOrder.Insert (index, key);

			OnCollectionChanged (new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Add, new KeyValuePair<TKey, TValue> (key, value), index));
			this.keys.SignalCollectionChanged (new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, key, index));
			this.values.SignalCollectionChanged (new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Add, value, index));
		}

		public void Replace (TKey key, TValue value)
		{
			int index = this.keyOrder.IndexOf (key);
			if (index == -1) {
				Add (key, value);
				return;
			}

			TValue oldValue = this.dict[key];
			this.dict[key] = value;

			OnCollectionChanged (new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Replace, new KeyValuePair<TKey, TValue> (key, value), new KeyValuePair<TKey, TValue> (key, oldValue), index));
			this.values.SignalCollectionChanged (new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Replace, value, oldValue, index));
		}

		public void RemoveAt (int index)
		{
			TKey key = this.keyOrder[index];
			TValue value = this.dict[key];
			this.keyOrder.RemoveAt (index);
			this.dict.Remove (key);

			OnCollectionChanged (new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Remove, new KeyValuePair<TKey, TValue> (key, value), index));
			this.keys.SignalCollectionChanged (new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Remove, key, index));
			this.values.SignalCollectionChanged (new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Remove, value, index));
		}

		public bool Remove (TKey key)
		{
			int index = this.keyOrder.IndexOf (key);
			if (index == -1)
				return false;

			TValue value = this.dict[key];
			this.dict.Remove (key);
			this.keyOrder.Remove (key);

			OnCollectionChanged (new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Remove, new KeyValuePair<TKey, TValue> (key, value), index));
			this.keys.SignalCollectionChanged (new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Remove, key, index));
			this.values.SignalCollectionChanged (new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Remove, value, index));

			return true;
		}

		bool ICollection<KeyValuePair<TKey, TValue>>.Remove (KeyValuePair<TKey, TValue> item)
		{
			return Remove(item.Key);
		}

		public bool TryGetValue (TKey key, out TValue value)
		{
			return this.dict.TryGetValue (key, out value);
		}

		void ICollection<KeyValuePair<TKey, TValue>>.CopyTo (KeyValuePair<TKey, TValue>[] array, int arrayIndex)
		{
			((ICollection<KeyValuePair<TKey, TValue>>)this.dict).CopyTo (array, arrayIndex);
		}

		public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
		{
			return this.dict.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		private readonly List<TKey> keyOrder;
		private readonly ReadOnlyObservableCollection<TKey> keys;
		private readonly ReadOnlyObservableCollection<TValue> values;
		private readonly Dictionary<TKey, TValue> dict;

		ICollection<TKey> IDictionary<TKey, TValue>.Keys
		{
			get { return this.keys; }
		}

		ICollection<TValue> IDictionary<TKey, TValue>.Values
		{
			get { return this.values; }
		}

		private void OnCollectionChanged (NotifyCollectionChangedEventArgs e)
		{
			NotifyCollectionChangedEventHandler handler = this.CollectionChanged;
			if (handler != null)
				handler (this, e);
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

			public void SignalCollectionChanged (NotifyCollectionChangedEventArgs e)
			{
				if (e == null)
					throw new ArgumentNullException ("e");

				var changed = CollectionChanged;
				if (changed != null)
					changed (this, e);
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