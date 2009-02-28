using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gablarski
{
	public class ReadOnlyDictionary<TKey, TValue>
		: IDictionary<TKey, TValue>
	{
		#region Constructors
		public ReadOnlyDictionary (IEnumerable<TValue> elements, Func<TValue, TKey> keySelector)
			: this (elements.ToDictionary(keySelector))
		{
		}

		public ReadOnlyDictionary (IDictionary<TKey, TValue> dict)
		{
			this.dict = dict;
		}
		#endregion

		#region IDictionary<TKey,TValue> Members

		public void Add (TKey key, TValue value)
		{
			throw new NotSupportedException ();
		}

		public bool ContainsKey (TKey key)
		{
			return this.dict.ContainsKey (key);
		}

		public ICollection<TKey> Keys
		{
			get { return this.dict.Keys; }
		}

		public bool Remove (TKey key)
		{
			throw new NotSupportedException ();
		}

		public bool TryGetValue (TKey key, out TValue value)
		{
			return this.dict.TryGetValue (key, out value);
		}

		public ICollection<TValue> Values
		{
			get { return this.dict.Values; }
		}

		public TValue this[TKey key]
		{
			get
			{
				return this.dict[key];
			}

			set
			{
				throw new NotSupportedException ();
			}
		}

		#endregion

		#region ICollection<KeyValuePair<TKey,TValue>> Members

		public void Add (KeyValuePair<TKey, TValue> item)
		{
			throw new NotSupportedException();
		}

		public void Clear ()
		{
			throw new NotSupportedException();
		}

		public bool Contains (KeyValuePair<TKey, TValue> item)
		{
			return this.dict.Contains (item);
		}

		public void CopyTo (KeyValuePair<TKey, TValue>[] array, int arrayIndex)
		{
			this.dict.CopyTo (array, arrayIndex);
		}

		public int Count
		{
			get { return this.dict.Count; }
		}

		public bool IsReadOnly
		{
			get { return true; }
		}

		public bool Remove (KeyValuePair<TKey, TValue> item)
		{
			throw new NotSupportedException ();
		}

		#endregion

		#region IEnumerable<KeyValuePair<TKey,TValue>> Members

		public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator ()
		{
			return this.dict.GetEnumerator ();
		}

		#endregion

		#region IEnumerable Members

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator ()
		{
			return ((System.Collections.IEnumerable)this.dict).GetEnumerator ();
		}

		#endregion

		private readonly IDictionary<TKey, TValue> dict;
	}
}
