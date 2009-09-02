using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Gablarski.Audio;
using Gablarski.Messages;

namespace Gablarski.Client
{
	public class ClientSourceManager
		: IAudioReceiver, IEnumerable<AudioSource>, INotifyCollectionChanged
	{
		protected internal ClientSourceManager (IClientContext context)
		{
			if (context == null)
				throw new ArgumentNullException ("context");

			this.context = context;
		}

		#region Events
		/// <summary>
		/// A new  or updated source list has been received.
		/// </summary>
		public event EventHandler<ReceivedListEventArgs<AudioSource>> ReceivedSourceList;

		/// <summary>
		/// A new audio source has been received.
		/// </summary>
		public event EventHandler<ReceivedAudioSourceEventArgs> ReceivedAudioSource;

		/// <summary>
		/// An audio source was removed.
		/// </summary>
		public event EventHandler<ReceivedListEventArgs<AudioSource>> AudioSourcesRemoved;

		/// <summary>
		/// An audio source started playing.
		/// </summary>
		public event EventHandler<AudioSourceEventArgs> AudioSourceStarted;

		/// <summary>
		/// An audio source stopped playing.
		/// </summary>
		public event EventHandler<AudioSourceEventArgs> AudioSourceStopped;

		/// <summary>
		/// An audio source has been muted.
		/// </summary>
		public event EventHandler<AudioSourceEventArgs> AudioSourceMuted;

		/// <summary>
		/// Audio has been received.
		/// </summary>
		public event EventHandler<ReceivedAudioEventArgs> ReceivedAudio;

		/// <summary>
		/// The collection of sources has changed.
		/// </summary>
		public event NotifyCollectionChangedEventHandler CollectionChanged;
		#endregion

		/// <summary>
		/// Gets a listing of the sources that belong to the current user.
		/// </summary>
		public IEnumerable<ClientAudioSource> Mine
		{
			get { return this.OfType<ClientAudioSource>(); }
		}

		#region Implementation of IEnumerable

		/// <summary>
		/// Returns an enumerator that iterates through the collection.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
		/// </returns>
		/// <filterpriority>1</filterpriority>
		public IEnumerator<AudioSource> GetEnumerator()
		{
			if (this.sources == null)
				yield break;

			lock (this.sources)
			{
				if (this.sources == null)
					yield break;

				foreach (var source in this.sources.Values)
					yield return source;
			}
		}

		/// <summary>
		/// Returns an enumerator that iterates through a collection.
		/// </summary>
		/// <returns>
		/// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
		/// </returns>
		/// <filterpriority>2</filterpriority>
		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}

		#endregion

		/// <summary>
		/// Requests a channel with <paramref name="channels"/> and a default bitrate.
		/// </summary>
		/// <param name="channels">The number of channels to request. 1-2 is the valid range.</param>
		/// <param name="name">The user-local name of the source, used to identify the source later.</param>
		public void Request (string name, int channels)
		{
			Request (name, channels, 0);
		}

		/// <summary>
		/// Requests a channel with <paramref name="channels"/> and a <paramref name="targetBitrate"/>
		/// </summary>
		/// <param name="channels">The number of channels to request. 1-2 is the valid range.</param>
		/// <param name="targetBitrate">The target bitrate to request.</param>
		/// <param name="name">The user-local name of the source, used to identify the source later.</param>
		/// <remarks>
		/// The server may not agree with the bitrate you request, do not set up audio based on this
		/// target, but on the bitrate of the source you actually receive.
		/// </remarks>
		public void Request (string name, int channels, int targetBitrate)
		{
			this.context.Connection.Send (new RequestSourceMessage (name, channels, targetBitrate));
		}

		/// <summary>
		/// Clears the source manager of all sources.
		/// </summary>
		public void Clear()
		{
			lock (this.sources)
			{
				this.sources.Clear();
				OnCollectionChanged (new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Reset));
			}
		}

		private readonly IClientContext context;
		private Dictionary<int, AudioSource> sources = new Dictionary<int, AudioSource>();

		// We'll end up with new instances from the outside world, we can update our
		// own instances no problem.
		internal void UpdateSourceFromExternal (AudioSource updatedSource)
		{
			NotifyCollectionChangedEventArgs collectionChanged;

			lock (sources)
			{
				AudioSource source;
				if (!sources.TryGetValue (updatedSource.Id, out source))
				{
					sources[updatedSource.Id] = source = updatedSource;
					collectionChanged = new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Add, source);
				}
				else
				{
					CopySource (source, updatedSource);
					collectionChanged = new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Replace, updatedSource, source);
				}
			}

			OnCollectionChanged (collectionChanged);
		}

		internal void UpdateSourcesFromExternal (IEnumerable<AudioSource> updatedSources)
		{
			IEnumerable<AudioSource> updatedAndNew;
			IEnumerable<AudioSource> deleted;

			lock (sources)
			{
				updatedAndNew = updatedSources.Where (s => !this.sources.ContainsValue (s));
				updatedAndNew = updatedAndNew.Concat (this.sources.Values.Intersect (updatedSources)).ToList();
				deleted = this.sources.Values.Where (s => !updatedSources.Contains (s)).ToList();
			}

			foreach (var s in updatedAndNew)
				UpdateSourceFromExternal (s);

			foreach (var d in deleted)
			{
				lock (sources)
				{
					sources.Remove (d.Id);
				}

				OnCollectionChanged (new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Remove, d));
			}
		}

		private void CopySource (AudioSource target, AudioSource updatedSource)
		{
			target.Name = updatedSource.Name;
			target.Id = updatedSource.Id;
			target.OwnerId = updatedSource.OwnerId;
			target.Bitrate = updatedSource.Bitrate;
			target.Muted = updatedSource.Muted;

			target.Channels = updatedSource.Channels;
			target.Frequency = updatedSource.Frequency;
			target.FrameSize = updatedSource.FrameSize;
		}

		internal void OnSourceListReceivedMessage (MessageReceivedEventArgs e)
		{
		    var msg = (SourceListMessage)e.Message;

			lock (sources)
			{
				UpdateSourcesFromExternal (msg.Sources.Select (s => (s.OwnerId == context.CurrentUser.UserId) ? new ClientAudioSource (s, this.context.Connection) : s));
			}

			OnReceivedSourceList (new ReceivedListEventArgs<AudioSource> (msg.Sources));
		}

		internal void OnSourceResultMessage (MessageReceivedEventArgs e)
		{
		    var sourceMessage = (SourceResultMessage)e.Message;
			var source = sourceMessage.Source;
			if (sourceMessage.SourceResult == SourceResult.Succeeded || sourceMessage.SourceResult == SourceResult.NewSource)
		    {
	        	source = (source.OwnerId.Equals (context.CurrentUser.UserId))
		        	         	? new ClientAudioSource (source, this.context.Connection)
		        	         	: source;
					
				UpdateSourceFromExternal (source);
		    }

		    OnReceivedSource (new ReceivedAudioSourceEventArgs (source, sourceMessage.SourceResult));
		}

		internal void OnSourcesRemovedMessage (MessageReceivedEventArgs e)
		{
			var sourceMessage = (SourcesRemovedMessage)e.Message;

			List<AudioSource> removed = new List<AudioSource>();
			lock (sources)
			{
				foreach (int id in sourceMessage.SourceIds)
				{
					if (!this.sources.ContainsKey (id))
						continue;

					removed.Add (this.sources[id]);
					this.sources.Remove (id);
				}
			}

			OnSourcesRemoved (new ReceivedListEventArgs<AudioSource> (removed));
			OnCollectionChanged (new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Remove, removed));
		}

		internal AudioSource GetSource (int sourceId)
		{
			AudioSource source = null;
			lock (this.sources)
			{
				if (this.sources != null && this.sources.ContainsKey (sourceId))
					source = this.sources[sourceId];
			}

			return source;
		}

		internal void OnAudioSourceStateChangedMessage (MessageReceivedEventArgs e)
		{
			var msg = (AudioSourceStateChangeMessage)e.Message;

			var source = GetSource (msg.SourceId);

			if (source != null)
			{
				if (msg.Starting)
					OnAudioSourceStarted (new AudioSourceEventArgs (source));
				else
					OnAudioSourceStopped (new AudioSourceEventArgs (source));
			}
		}

		internal void OnAudioDataReceivedMessage (MessageReceivedEventArgs e)
		{
			var msg = (AudioDataReceivedMessage)e.Message;

			var source = GetSource (msg.SourceId);
			if (source != null)
				OnReceivedAudio (new ReceivedAudioEventArgs (source, msg.Sequence, msg.Data));
		}

		#region Event Invokers
		protected virtual void OnAudioSourceStarted (AudioSourceEventArgs e)
		{
			var started = this.AudioSourceStarted;
			if (started != null)
				started (this, e);
		}

		protected virtual void OnAudioSourceStopped (AudioSourceEventArgs e)
		{
			var stopped = this.AudioSourceStopped;
			if (stopped != null)
				stopped (this, e);
		}

		protected virtual void OnReceivedAudio (ReceivedAudioEventArgs e)
		{
			var received = this.ReceivedAudio;
			if (received != null)
				received (this, e);
		}

		protected virtual void OnReceivedSource (ReceivedAudioSourceEventArgs e)
		{
			var received = this.ReceivedAudioSource;
			if (received != null)
				received (this, e);
		}

		protected virtual void OnReceivedSourceList (ReceivedListEventArgs<AudioSource> e)
		{
			var received = this.ReceivedSourceList;
			if (received != null)
				received (this, e);
		}

		protected virtual void OnSourcesRemoved (ReceivedListEventArgs<AudioSource> e)
		{
			var removed = this.AudioSourcesRemoved;
			if (removed != null)
				removed (this, e);
		}

		protected virtual void OnCollectionChanged (NotifyCollectionChangedEventArgs e)
		{
			var changed = this.CollectionChanged;
			if (changed != null)
				changed (this, e);
		}
		#endregion
	}

	#region Event Args
	public class ReceivedAudioSourceEventArgs
		: EventArgs
	{
		public ReceivedAudioSourceEventArgs (AudioSource source, SourceResult result)
		{
			this.Result = result;
			this.Source = source;
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
		/// Gets the media source of the event.
		/// </summary>
		public AudioSource Source
		{
			get;
			private set;
		}
	}
	#endregion
}