//
// AudioSourceManager.cs
//
// Author:
//   Eric Maupin <me@ermau.com>
//
// Copyright (c) 2009-2011, Eric Maupin
// Copyright (c) 2011-2014, Xamarin Inc.
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
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using Cadenza.Collections;
using Gablarski.Audio;

namespace Gablarski
{
	public class AudioSourceManager
		: IReadOnlyDictionary<int, AudioSource>, INotifyCollectionChanged
	{
		public event NotifyCollectionChangedEventHandler CollectionChanged;

		public int Count
		{
			get
			{
				lock (this.syncRoot)
					return Sources.Count;
			}
		}

		public AudioSource this [int key]
		{
			get
			{
				AudioSource source;
				lock (this.syncRoot)
				{
					if (!Sources.TryGetValue (key, out source))
						return null;
				}

				return source;
			}
		}

		public IEnumerable<int> Keys
		{
			get
			{
				lock (this.syncRoot)
					return new ReadOnlyCollection<int> (Sources.Keys.ToList());
			}
		}

		public IEnumerable<AudioSource> Values
		{
			get
			{
				lock (this.syncRoot)
					return new ReadOnlyCollection<AudioSource> (Sources.Values.ToList());
			}
		}

		/// <summary>
		/// Gets the audio sources owned by <paramref name="user"/>.
		/// </summary>
		/// <param name="user">The owner to find sources for.</param>
		/// <returns>An empty enumerable if <paramref name="user"/> doesn't own any sources, otherwise the owned sources.</returns>
		public IEnumerable<AudioSource> this [IUserInfo user]
		{
			get
			{
				lock (this.syncRoot)
					return OwnedSources[user.UserId].ToList();
			}
		}

		public bool ContainsKey (int key)
		{
			lock (this.syncRoot)
				return Sources.ContainsKey (key);
		}

		public bool TryGetValue (int key, out AudioSource value)
		{
			lock (this.syncRoot)
				return Sources.TryGetValue (key, out value);
		}

		/// <summary>
		/// Toggles mute for the <paramref name="source"/>.
		/// </summary>
		/// <param name="source">The source to toggle mute for.</param>
		/// <returns><c>true</c> if <paramref name="source"/> was muted, <c>false</c> if not or not found.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="source"/> is <c>null</c>.</exception>
		public bool ToggleMute (AudioSource source)
		{
			if (source == null)
				throw new ArgumentNullException ("source");

			AudioSource newSource, actual;
			int index;
			lock (this.syncRoot) {
				if (!Sources.TryGetValue (source.Id, out actual))
					return false;

				newSource = new AudioSource (actual) {
					IsMuted = !actual.IsMuted
				};

				OwnedSources.Remove (source.OwnerId, source);

				index = Sources.IndexOf (newSource.Id);
				Sources.RemoveAt (index);

				OwnedSources.Add (source.OwnerId, source);
				Sources.Insert (index, newSource.Id, newSource);
			}

			OnCollectionChanged (new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Replace, newSource, actual, index));
			return newSource.IsMuted;
		}

		/// <summary>
		/// Clears the manager of all sources.
		/// </summary>
		public virtual void Clear()
		{
			lock (this.syncRoot) {
				OwnedSources.Clear();
				Sources.Clear();
			}

			OnCollectionChanged (new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Reset));
		}

		/// <summary>
		/// Removes the audio <paramref name="source"/>.
		/// </summary>
		/// <param name="source">The source to remove.</param>
		/// <returns><c>true</c> if the source was found and removed, <c>false otherwise.</c></returns>
		/// <exception cref="ArgumentNullException"><paramref name="source"/> is <c>null</c>.</exception>
		public virtual bool Remove (AudioSource source)
		{
			if (source == null)
				throw new ArgumentNullException ("source");

			AudioSource actual;
			int index;
			lock (this.syncRoot) {
				if (!Sources.TryGetValue (source.Id, out actual))
					return false;
			
				OwnedSources.Remove (source.OwnerId, source);
				index = Sources.IndexOf (source.Id);
				
				Sources.RemoveAt (index);
			}

			OnCollectionChanged (new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Remove, actual, index));
			return true;
		}

		/// <summary>
		/// Removes all audio sources for <paramref name="user"/>.
		/// </summary>
		/// <param name="user">The user to remove all sources for.</param>
		/// <returns><c>true</c> if any sources belonging to <paramref name="user"/> were found.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="user"/> is <c>null</c>.</exception>
		public virtual bool Remove (IUserInfo user)
		{
			if (user == null)
				throw new ArgumentNullException ("user");

			bool any = false;
			lock (this.syncRoot) {
				foreach (AudioSource source in OwnedSources[user.UserId].ToList()) {
					bool removed = Remove (source);
					if (removed)
						any = true;
				}
			}

			return any;
		}

		public IEnumerator<AudioSource> GetEnumerator()
		{
			lock (this.syncRoot)
				return Sources.Values.ToList().GetEnumerator();
		}

		IEnumerator<KeyValuePair<int, AudioSource>> IEnumerable<KeyValuePair<int, AudioSource>>.GetEnumerator()
		{
			lock (this.syncRoot)
				return new OrderedDictionary<int, AudioSource> (Sources).GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		protected readonly object syncRoot = new object();

		protected readonly MutableLookup<int, AudioSource> OwnedSources = new MutableLookup<int, AudioSource>();
		protected readonly OrderedDictionary<int, AudioSource> Sources = new OrderedDictionary<int, AudioSource>();

		protected virtual void OnCollectionChanged (NotifyCollectionChangedEventArgs e)
		{
			var handler = CollectionChanged;
			if (handler != null)
				handler (this, e);
		}
	}
}