using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gablarski
{
	public class PairCollection<T1, T2>
		: IEnumerable<KeyValuePair<T1, T2>>
	{
		public PairCollection ()
		{
			this.ones = new Dictionary<T1, T2> ();
			this.twos = new Dictionary<T2, T1> ();
		}

		public PairCollection (int capacity)
		{
			this.ones = new Dictionary<T1, T2> (capacity);
			this.twos = new Dictionary<T2, T1> (capacity);
		}

		public IEnumerable<T1> Ones
		{
			get { return this.ones.Keys; }
		}

		public IEnumerable<T2> Twos
		{
			get { return this.twos.Keys; }
		}

		public T1 this[T2 t2]
		{
			get { return this.twos[t2]; }
			set
			{
				this.twos[t2] = value;
				this.ones[value] = t2;
			}
		}

		public T2 this[T1 t1]
		{
			get { return this.ones[t1]; }
			set
			{
				this.ones[t1] = value;
				this.twos[value] = t1;
			}
		}

		public bool Contains (T1 one)
		{
			return this.ones.ContainsKey (one);
		}

		public bool Contains (T2 two)
		{
			return this.twos.ContainsKey (two);
		}

		public bool Remove (T1 one)
		{
			if (!this.ones.ContainsKey (one))
				return false;

			T2 two = this.ones[one];
			this.ones.Remove (one);
			this.twos.Remove (two);

			return true;
		}

		public bool Remove (T2 two)
		{
			if (!this.twos.ContainsKey (two))
				return false;

			T1 one = this.twos[two];
			this.twos.Remove (two);
			this.ones.Remove (one);

			return true;
		}

		private readonly Dictionary<T1, T2> ones;
		private readonly Dictionary<T2, T1> twos;

		#region IEnumerable Members

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator ()
		{
			return this.GetEnumerator ();	
		}

		#endregion

		#region IEnumerable<KeyValuePair<T1,T2>> Members

		public IEnumerator<KeyValuePair<T1, T2>> GetEnumerator ()
		{
			return this.ones.GetEnumerator ();
		}

		#endregion
	}
}