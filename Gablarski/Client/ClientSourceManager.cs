using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Gablarski.Media.Sources;
using Gablarski.Messages;

namespace Gablarski.Client
{
	public class ClientSourceManager
		: IEnumerable<MediaSourceBase>
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
		public event EventHandler<ReceivedListEventArgs<MediaSourceBase>> ReceivedSourceList;

		/// <summary>
		/// A new media source has been received.
		/// </summary>
		public event EventHandler<ReceivedSourceEventArgs> ReceivedSource;

		/// <summary>
		/// A media source was removed.
		/// </summary>
		public event EventHandler<ReceivedListEventArgs<MediaSourceBase>> SourcesRemoved;

		/// <summary>
		/// Audio has been received.
		/// </summary>
		public event EventHandler<ReceivedAudioEventArgs> ReceivedAudio;
		#endregion

		/// <summary>
		/// Gets a listing of the sources that belong to the current user.
		/// </summary>
		public IEnumerable<ClientMediaSource> Mine
		{
			get { return this.OfType<ClientMediaSource>(); }
		}

		#region Implementation of IEnumerable

		/// <summary>
		/// Returns an enumerator that iterates through the collection.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
		/// </returns>
		/// <filterpriority>1</filterpriority>
		public IEnumerator<MediaSourceBase> GetEnumerator()
		{
			if (this.sources == null)
				yield break;

			lock (this.sourceLock)
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
		public void Request (int channels)
		{
			Request (channels, 0);
		}

		/// <summary>
		/// Requests a channel with <paramref name="channels"/> and a <paramref name="targetBitrate"/>
		/// </summary>
		/// <param name="channels">The number of channels to request. 1-2 is the valid range.</param>
		/// <param name="targetBitrate">The target bitrate to request.</param>
		/// <remarks>
		/// The server may not agree with the bitrate you request, do not set up audio based on this
		/// target, but on the bitrate of the source you actually receive.
		/// </remarks>
		public void Request (int channels, int targetBitrate)
		{
			this.context.Connection.Send (new RequestSourceMessage (channels, targetBitrate));
		}

		private readonly IClientContext context;
		private readonly object sourceLock = new object();
		private Dictionary<int, MediaSourceBase> sources;

		internal void OnSourceListReceivedMessage (MessageReceivedEventArgs e)
		{
		    var msg = (SourceListMessage)e.Message;

			lock (sourceLock)
			{
				this.sources = msg.Sources.ToDictionary (s => s.Id, s => (s.OwnerId == context.CurrentUser.UserId) ? new ClientMediaSource (s, this.context.Connection) : s);
			}

			OnReceivedSourceList (new ReceivedListEventArgs<MediaSourceBase> (msg.Sources));
		}

		internal void OnSourceResultMessage (MessageReceivedEventArgs e)
		{
		    var sourceMessage = (SourceResultMessage)e.Message;
			var source = sourceMessage.Source;
			if (sourceMessage.SourceResult == SourceResult.Succeeded || sourceMessage.SourceResult == SourceResult.NewSource)
		    {
		        lock (sourceLock)
		        {
		        	if (sources == null)
						sources = new Dictionary<int, MediaSourceBase>();

		        	source = (source.OwnerId.Equals (context.CurrentUser.UserId))
		        	         	? new ClientMediaSource (source, this.context.Connection)
		        	         	: source;
					
					sources.Add (source.Id, source);
		        }
		    }

		    OnReceivedSource (new ReceivedSourceEventArgs (source, sourceMessage.SourceResult));
		}

		internal void OnSourcesRemovedMessage (MessageReceivedEventArgs e)
		{
			var sourceMessage = (SourcesRemovedMessage)e.Message;

			List<MediaSourceBase> removed = new List<MediaSourceBase>();
			lock (sourceLock)
			{
				foreach (int id in sourceMessage.SourceIds)
				{
					if (!this.sources.ContainsKey (id))
						continue;

					removed.Add (this.sources[id]);
					this.sources.Remove (id);
				}
			}

			this.OnSourcesRemoved (new ReceivedListEventArgs<MediaSourceBase> (removed));
		}

		internal void OnAudioDataReceivedMessage (MessageReceivedEventArgs e)
		{
			var msg = (AudioDataReceivedMessage)e.Message;

			AudioSource source = null;
			lock (this.sourceLock)
			{
				if (this.sources != null && this.sources.ContainsKey (msg.SourceId))
					source = (this.sources[msg.SourceId] as AudioSource);
			}

			if (source != null)
				OnReceivedAudio (new ReceivedAudioEventArgs (source, source.Decode (msg.Data)));
		}

		#region Event Invokers
		protected virtual void OnReceivedAudio (ReceivedAudioEventArgs e)
		{
			var received = this.ReceivedAudio;
			if (received != null)
				received (this, e);
		}

		protected virtual void OnReceivedSource (ReceivedSourceEventArgs e)
		{
			var received = this.ReceivedSource;
			if (received != null)
				received (this, e);
		}

		protected virtual void OnReceivedSourceList (ReceivedListEventArgs<MediaSourceBase> e)
		{
			var received = this.ReceivedSourceList;
			if (received != null)
				received (this, e);
		}

		protected virtual void OnSourcesRemoved (ReceivedListEventArgs<MediaSourceBase> e)
		{
			var removed = this.SourcesRemoved;
			if (removed != null)
				removed (this, e);
		}
		#endregion
	}

	#region Event Args
	public class ReceivedAudioEventArgs
		: EventArgs
	{
		public ReceivedAudioEventArgs (AudioSource source, byte[] data)
		{
			this.Source = source;
			this.AudioData = data;
		}

		/// <summary>
		/// Gets the media source audio was received for.
		/// </summary>
		public AudioSource Source
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets the audio data.
		/// </summary>
		public byte[] AudioData
		{
			get;
			private set;
		}
	}

	public class ReceivedSourceEventArgs
		: EventArgs
	{
		public ReceivedSourceEventArgs (MediaSourceBase source, SourceResult result)
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
		public MediaSourceBase Source
		{
			get;
			private set;
		}
	}
	#endregion
}