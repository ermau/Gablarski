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
		public ClientSourceManager (IClientContext context)
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
		/// Audio has been received.
		/// </summary>
		public event EventHandler<ReceivedAudioEventArgs> ReceivedAudio;
		#endregion

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
			throw new NotImplementedException();
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

		public void Request (int channels)
		{
			Request (channels, 0);
		}

		public void Request (int channels, int targetBitrate)
		{
			this.context.Connection.Send (new RequestSourceMessage (channels, targetBitrate));
		}

		//public void RequestSource (Type mediaSourceType, byte channels)
		//{
		//    if (mediaSourceType == null)
		//        throw new ArgumentNullException ("mediaSourceType");
		//    if (mediaSourceType.GetInterface ("MediaSourceBase") == null)
		//        throw new InvalidOperationException ("Can not request a source that is not a media source.");

		//    lock (sourceLock)
		//    {
		//        if (this.clientSources.Values.Any (s => s.GetType () == mediaSourceType))
		//            throw new InvalidOperationException ("Client already owns a source of this type.");
		//    }

		//    this.Connection.Send (new RequestSourceMessage (mediaSourceType, channels));
		//}

		private readonly IClientContext context;
		private readonly object sourceLock = new object();
		private Dictionary<int, MediaSourceBase> sources;

		internal void OnSourceListReceivedMessage (MessageReceivedEventArgs e)
		{
		    var msg = (SourceListMessage)e.Message;

			lock (sourceLock)
			{
				this.sources = msg.Sources.ToDictionary (s => s.Id);
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

					sources.Add (source.Id, source);
		        }
		    }

		    OnReceivedSource (new ReceivedSourceEventArgs (source, sourceMessage.SourceResult));
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
				OnReceivedAudio (new ReceivedAudioEventArgs (source, msg.Data));
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
			set;
		}

		/// <summary>
		/// Gets the audio data.
		/// </summary>
		public byte[] AudioData
		{
			get;
			set;
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

		public SourceResult Result
		{
			get;
			set;
		}

		/// <summary>
		/// Gets the media source of the event.
		/// </summary>
		public MediaSourceBase Source
		{
			get;
			set;
		}
	}
	#endregion
}