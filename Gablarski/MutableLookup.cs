using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gablarski
{
	/// <summary>
	/// A mutable lookup implementing <see cref="ILookup`2"/>
	/// </summary>
	/// <typeparam name="TKey">The lookup key.</typeparam>
	/// <typeparam name="TElement">The elements under each <typeparamref name="TKey"/>.</typeparam>
	public class MutableLookup<TKey, TElement>
		: ILookup<TKey, TElement>
	{
		/// <summary>
		/// Adds <paramref name="element"/> under the specified <paramref name="key"/>. <paramref name="key"/> does not need to exist.
		/// </summary>
		/// <param name="key">The key to add <paramref name="element"/> under.</param>
		/// <param name="element">The element to add.</param>
		public void Add (TKey key, TElement element)
		{
			if (!this.groupings.ContainsKey (key))
				this.groupings.Add (key, new MutableLookupGrouping (key));

			this.groupings[key].Add (element);
		}

		/// <summary>
		/// Removes <paramref name="element"/> from the <paramref name="key"/>.
		/// </summary>
		/// <param name="key">The key that <paramref name="element"/> is located under.</param>
		/// <param name="element">The element to remove from <paramref name="key"/>. </param>
		/// <returns><c>true</c> if <paramref name="key"/> and <paramref name="element"/> existed, <c>false</c> if not.</returns>
		public bool Remove (TKey key, TElement element)
		{
			if (!this.groupings.ContainsKey (key))
				return false;

			return this.groupings[key].Remove (element);
		}

		/// <summary>
		/// Removes <paramref name="key"/> from the lookup.
		/// </summary>
		/// <param name="key">They to remove.</param>
		/// <returns><c>true</c> if <paramref name="key"/> existed.</returns>
		public bool Remove (TKey key)
		{
			return this.groupings.Remove (key);
		}

		#region ILookup Members
		/// <summary>
		/// Gets the number of groupings.
		/// </summary>
		public int Count
		{
			get { return this.groupings.Count; }
		}

		/// <summary>
		/// Gets the elements for <paramref name="key"/>.
		/// </summary>
		/// <param name="key">The key to get the elements for.</param>
		/// <returns>The elements under <paramref name="key"/>.</returns>
		public IEnumerable<TElement> this[TKey key]
		{
			get { return this.groupings[key]; }
		}

		/// <summary>
		/// Gets whether or not there's a grouping for <paramref name="key"/>.
		/// </summary>
		/// <param name="key">The key to check for.</param>
		/// <returns><c>true</c> if <paramref name="key"/> is present.</returns>
		public bool Contains (TKey key)
		{
			return this.groupings.ContainsKey (key);
		}

		public IEnumerator<IGrouping<TKey, TElement>> GetEnumerator ()
		{
			return this.groupings.Values.Cast<IGrouping<TKey, TElement>> ().GetEnumerator();
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator ()
		{
			return this.GetEnumerator ();
		}
		#endregion

		private readonly Dictionary<TKey, MutableLookupGrouping> groupings = new Dictionary<TKey, MutableLookupGrouping> ();

		private class MutableLookupGrouping
			: IGrouping<TKey, TElement>
		{
			public MutableLookupGrouping (TKey key)
			{
				this.Key = key;
			}

			public TKey Key
			{
				get;
				private set;
			}

			public void Add (TElement element)
			{
				this.elements.Add (element);
			}

			public bool Remove (TElement element)
			{
				return this.elements.Remove (element);
			}

			public IEnumerator<TElement> GetEnumerator ()
			{
				return this.elements.GetEnumerator ();
			}

			System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator ()
			{
				return this.GetEnumerator ();
			}

			private List<TElement> elements = new List<TElement> ();
		}
	}
}