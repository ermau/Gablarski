using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Gablarski.Annotations;

namespace Gablarski.ViewModels
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

			this.view = new List<T> (source.Where (filter));
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

		private readonly List<T> view = new List<T>();
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