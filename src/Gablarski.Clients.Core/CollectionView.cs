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
using System.ComponentModel;
using System.Globalization;
using System.Threading;
using System.Windows.Data;

namespace Gablarski.Clients
{
	public sealed class LambdaConverter<TSource, TValue>
		: IValueConverter
	{
		public LambdaConverter (Func<TSource, TValue> convert, Func<TValue, TSource> convertBack)
		{
			if (convert == null)
				throw new ArgumentNullException ("convert");
			if (convertBack == null)
				throw new ArgumentNullException ("convertBack");

			this.convert = convert;
			this.convertBack = convertBack;
		}

		public object Convert (object value, Type targetType, object parameter, CultureInfo culture)
		{
			return this.convert ((TSource) value);
		}

		public object ConvertBack (object value, Type targetType, object parameter, CultureInfo culture)
		{
			return this.convertBack ((TValue)value);
		}

		private readonly Func<TSource, TValue> convert;
		private readonly Func<TValue, TSource> convertBack;
	}

	public sealed class CollectionView<T>
		: INotifyCollectionChanged, IReadOnlyList<T>
	{
		public CollectionView (IEnumerable itemSource)
		{
			if (itemSource == null)
				throw new ArgumentNullException ("itemSource");

			this.syncContext = SynchronizationContext.Current;
			this.itemSource = itemSource;

			var incc = itemSource as INotifyCollectionChanged;
			if (incc != null)
				incc.CollectionChanged += OnCollectionChanged;

			Reset();
		}

		public CollectionView (IEnumerable itemSource, IValueConverter itemConverter)
		{
			if (itemConverter == null)
				throw new ArgumentNullException ("itemConverter");
			if (itemSource == null)
				throw new ArgumentNullException ("itemSource");

			this.syncContext = SynchronizationContext.Current;
			this.itemSource = itemSource;

			var incc = itemSource as INotifyCollectionChanged;
			if (incc != null)
				incc.CollectionChanged += OnCollectionChanged;

			ItemConverter = itemConverter;
		}

		public event NotifyCollectionChangedEventHandler CollectionChanged;

		public IValueConverter ItemConverter
		{
			get { return this.itemConverter; }
			set
			{
				if (this.itemConverter == value)
					return;

				this.itemConverter = value;
				Reset();
			}
		}

		public Func<object, bool> ItemFilter
		{
			get { return this.itemFilter; }
			set
			{
				if (this.itemFilter == value)
					return;

				this.itemFilter = value;
				Reset();
			}
		}

		public int Count
		{
			get { return this.items.Count; }
		}

		public T this [int index]
		{
			get { return this.items[index]; }
		}

		public IEnumerator<T> GetEnumerator()
		{
			return this.items.GetEnumerator();
		}

		private readonly SynchronizationContext syncContext;
		private readonly List<T> items = new List<T>();
		private readonly IEnumerable itemSource;
		private IValueConverter itemConverter;
		private Func<object, bool> itemFilter;

		private void OnCollectionChanged (object sender, NotifyCollectionChangedEventArgs e)
		{
			SendOrPostCallback action = s => {
				var args = (NotifyCollectionChangedEventArgs) s;
				switch (e.Action) {
					/*case NotifyCollectionChangedAction.Add:
						if (e.NewStartingIndex == -1)
							goto case NotifyCollectionChangedAction.Reset;

						args = AddItems (e);
						break;

					case NotifyCollectionChangedAction.Remove:
						if (e.OldStartingIndex == -1)
							goto case NotifyCollectionChangedAction.Reset;

						args = RemoveItems (e);
						break;

					case NotifyCollectionChangedAction.Replace:
						if (e.OldStartingIndex == -1)
							goto case NotifyCollectionChangedAction.Reset;

						args = ReplaceItems (e);
						break;*/

					default:
					case NotifyCollectionChangedAction.Reset:
						Reset();
						args = new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Reset);
						break;
				}

				OnCollectionChanged (args);
			};

			if (this.syncContext != null)
				this.syncContext.Post (action, e);
			else
				action (e);
		}

		private NotifyCollectionChangedEventArgs ReplaceItems (NotifyCollectionChangedEventArgs e)
		{
			List<T> oldConverted = new List<T> (e.OldItems.Count);
			List<T> newConverted = new List<T> (e.OldItems.Count);

			for (int i = 0; i < e.OldItems.Count; i++) {
				object element = e.OldItems[i];
				DetatchListener (element);

				oldConverted.Add (this.items[i + e.OldStartingIndex]);

				object newItem = e.NewItems[i];
				if (ItemConverter != null)
					newItem = ItemConverter.Convert (newItem, typeof (T), null, null);

				newConverted.Add ((T) newItem);
				this.items[i] = (T) newItem;
			}

			return new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Replace, newConverted, oldConverted, e.NewStartingIndex);
		}

		private NotifyCollectionChangedEventArgs RemoveItems (NotifyCollectionChangedEventArgs e)
		{
			List<T> converted = new List<T> (e.OldItems.Count);

			for (int i = 0; i < e.OldItems.Count; i++) {
				object element = e.OldItems[i];
				DetatchListener (element);

				converted.Add (this.items[i]);
			}

			this.items.RemoveRange (e.OldStartingIndex, e.OldItems.Count);

			return new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Remove, converted, e.OldStartingIndex);
		}

		private NotifyCollectionChangedEventArgs AddItems (NotifyCollectionChangedEventArgs e)
		{
			List<T> converted = new List<T> (e.NewItems.Count);

			for (int i = 0; i < e.NewItems.Count; i++) {
				object element = e.NewItems[i];
				AttachListener (element);

				if (ItemConverter != null)
					element = ItemConverter.Convert (element, typeof (T), null, null);

				converted.Add ((T) element);
				this.items.Insert (e.NewStartingIndex + i, (T) element);
			}

			return new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Add, converted, e.NewStartingIndex);;
		}

		private void OnCollectionChanged (NotifyCollectionChangedEventArgs e)
		{
			var handler = CollectionChanged;
			if (handler != null)
				handler (this, e);
		}

		private void Reset()
		{
			BindingOperations.AccessCollection (this.itemSource, ResetCore, false);
		}

		private void ResetCore()
		{
			foreach (T item in this.items)
				DetatchListener (item);

			this.items.Clear();

			foreach (object item in this.itemSource) {
				object element = item;
				AttachListener (element);

				if (ItemConverter != null)
					element = ItemConverter.Convert (element, typeof(T), null, null);

				this.items.Add ((T)element);
			}
		}

		private void DetatchListener (object item)
		{
			var inpc = item as INotifyPropertyChanged;
			if (inpc != null)
				inpc.PropertyChanged -= OnItemPropertyChanged;
		}

		private void OnItemPropertyChanged (object sender, PropertyChangedEventArgs e)
		{
			Reset();// TODO
		}

		private void AttachListener (object item)
		{
			if (ItemFilter == null)
				return;

			var inpc = item as INotifyPropertyChanged;
			if (inpc != null)
				inpc.PropertyChanged += OnItemPropertyChanged;
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}