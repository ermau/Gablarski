// Copyright (c) 2013, Eric Maupin
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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

namespace Gablarski.Clients
{
	internal class SelectedObservableCollection<TSource, TSelected>
		: ObservableCollection<TSelected>
	{
		private readonly Func<TSource, TSelected> selector;

		public SelectedObservableCollection (IEnumerable<TSource> source, Func<TSource, TSelected> selector)
		{
			if (source == null)
				throw new ArgumentNullException ("source");
			if (selector == null)
				throw new ArgumentNullException ("selector");

			foreach (TSelected element in source.Select (selector))
				Add (element);

			this.selector = selector;

			var changed = source as INotifyCollectionChanged;
			if (changed != null)
				changed.CollectionChanged += ChangedOnCollectionChanged;
		}

		private readonly Dictionary<TSource, List<TSelected>> maps = new Dictionary<TSource, List<TSelected>>();

		private void ChangedOnCollectionChanged (object sender, NotifyCollectionChangedEventArgs e)
		{
			switch (e.Action) {
				case NotifyCollectionChangedAction.Add:
					if (e.NewItems == null)
						return;

					foreach (TSource newItem in e.NewItems) {
						List<TSelected> selectedItems;
						if (!this.maps.TryGetValue (newItem, out selectedItems))
							this.maps[newItem] = selectedItems = new List<TSelected>();

						selectedItems.Add (this.selector (newItem));
					}

					break;

				case NotifyCollectionChangedAction.Remove:
					if (e.OldItems == null)
						return;

					foreach (TSource oldItem in e.OldItems) {
						List<TSelected> selectedItems;
						if (this.maps.TryGetValue (oldItem, out selectedItems)) {
							selectedItems.RemoveAt (0);

							if (selectedItems.Count == 0)
								this.maps.Remove (oldItem);
						}
					}

					break;

				case NotifyCollectionChangedAction.Reset:
					Clear();
					this.maps.Clear();
					break;
			}
		}
	}
}