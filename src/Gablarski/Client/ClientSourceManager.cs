// Copyright (c) 2011, Eric Maupin
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
using System.Linq;
using System.Threading;
using Gablarski.Audio;
using Gablarski.Messages;

namespace Gablarski.Client
{
	public class ClientSourceManager
		: AudioSourceManager, IClientSourceManager
	{
		protected internal ClientSourceManager (IClientContext context)
		{
			if (context == null)
				throw new ArgumentNullException ("context");

			this.context = context;
		}

		public object SyncRoot
		{
			get { return this.syncRoot; }
		}

		/// <summary>
		/// Gets a listing of the sources that belong to the current user.
		/// </summary>
		public IEnumerable<AudioSource> Mine
		{
			get
			{
				lock (syncRoot)
				{
					return this.Where (s => s.OwnerId == context.CurrentUser.UserId).ToList();
				}
			}
		}

		public void Add (AudioSource source)
		{
			Update (source);
		}

		public void Update (IEnumerable<AudioSource> updatedSources)
		{
			if (updatedSources == null)
				throw new ArgumentNullException ("updatedSources");

			IEnumerable<AudioSource> updatedAndNew;

			lock (syncRoot)
			{
				updatedAndNew = updatedSources.Where (s => !Sources.ContainsValue (s));
				updatedAndNew = updatedAndNew.Concat (Sources.Values.Intersect (updatedSources)).ToList();
				var deleted = Sources.Values.Where (s => !updatedSources.Contains (s)).ToList();

				foreach (var s in updatedAndNew)
					Update (s);

				foreach (var d in deleted)
				{
					lock (Sources)
						Sources.Remove (d.Id);
				}
			}
		}

		public void Update (AudioSource source)
		{
			if (source == null)
				throw new ArgumentNullException ("source");

			lock (syncRoot)
			{
				AudioSource oldSource;
				if (!Sources.TryGetValue (source.Id, out oldSource))
				{
					Sources[source.Id] = source;
					OwnedSources.Add (source.OwnerId, source);
				}
				else
					source.CopyTo (oldSource);
			}
		}

		public bool GetIsIgnored (AudioSource source)
		{
			if (source == null)
				throw new ArgumentNullException ("source");

			lock (syncRoot)
			{
				return ignoredSources.Contains (source);
			}
		}

		public bool ToggleIgnore (AudioSource source)
		{
			if (source == null)
				throw new ArgumentNullException ("source");

			lock (syncRoot)
			{
				if (!Sources.ContainsKey (source.Id))
					return false;

				bool ignored = ignoredSources.Contains (source);
				
				if (ignored)
					ignoredSources.Remove (source);
				else
					ignoredSources.Add (source);

				return !ignored;
			}
		}

		public override void Clear()
		{
			lock (syncRoot)
			{
				ignoredSources.Clear();
				base.Clear();
			}
		}

		private readonly IClientContext context;
		private readonly HashSet<AudioSource> ignoredSources = new HashSet<AudioSource>();
	}

	#region Event Args

	public class AudioSourceMutedEventArgs
		: AudioSourceEventArgs
	{
		public AudioSourceMutedEventArgs (AudioSource source, bool unmuted)
			: base (source)
		{
			this.Unmuted = unmuted;
		}

		public bool Unmuted { get; set; }
	}

	public class ReceivedAudioSourceEventArgs
		: EventArgs
	{
		public ReceivedAudioSourceEventArgs (string sourceName, AudioSource source, SourceResult result)
		{
			this.SourceName = sourceName;
			this.Result = result;
			this.Source = source;
		}

		/// <summary>
		/// Gets the name of the requested source.
		/// </summary>
		public string SourceName
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets the result of the source request.
		/// </summary>
		public SourceResult Result
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets the media source of the event. <c>null</c> if failed.
		/// </summary>
		public AudioSource Source
		{
			get;
			private set;
		}
	}

	#endregion
}