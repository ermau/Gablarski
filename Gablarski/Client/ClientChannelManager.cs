using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using Gablarski.Messages;

namespace Gablarski.Client
{
	public class ClientChannelManager
		: IEnumerable<ChannelInfo>, INotifyCollectionChanged
	{
		protected internal ClientChannelManager (IClientContext context)
		{
			if (context == null)
				throw new ArgumentNullException ("context");

			this.context = context;
		}

		#region Events
		/// <summary>
		/// The result of a channel edit request has been received.
		/// </summary>
		public event EventHandler<ChannelEditResultEventArgs> ReceivedChannelEditResult;

		/// <summary>
		/// A new or updated player list has been received.
		/// </summary>
		public event EventHandler<ReceivedListEventArgs<ChannelInfo>> ReceivedChannelList;

		public event NotifyCollectionChangedEventHandler CollectionChanged;
		#endregion

		/// <summary>Gets the channel with id <paramref name="channelId"/></summary>
		/// <param name="channelId">The id of the channel.</param>
		/// <returns><c>null</c> if no channel exists by the identifier.</returns>
		public ChannelInfo this[int channelId]
		{
			get
			{
				if (this.channels == null || this.channels.Count == 0)
					return null;

				lock (channelLock)
				{
					ChannelInfo channel;
					this.channels.TryGetValue (channelId, out channel);
					return channel;
				}
			}
		}

		/// <summary>
		/// Send a create channel request to the server.
		/// </summary>
		/// <param name="channel">The channel to create.</param>
		public void Create (ChannelInfo channel)
		{
			if (channel == null)
				throw new ArgumentNullException ("channel");

			if (channel.ChannelId != 0)
				throw new ArgumentException ("Can not create an existing channel", "channel");

			this.context.Connection.Send (new ChannelEditMessage (channel));
		}

		/// <summary>
		/// Sends an update request to the server for <paramref name="channel"/>.
		/// </summary>
		/// <param name="channel">The updated information for the channel.</param>
		public void Update (ChannelInfo channel)
		{
			if (channel == null)
				throw new ArgumentNullException ("channel");

			if (channel.ChannelId == 0)
				throw new ArgumentException ("channel must be an existing channel", "channel");

			this.context.Connection.Send (new ChannelEditMessage (channel));
		}

		/// <summary>
		/// Sends a delete channel request to the server.
		/// </summary>
		/// <param name="channel">The channel to delete.</param>
		public void Delete (ChannelInfo channel)
		{
			if (channel == null)
				throw new ArgumentNullException ("channel");

			if (channel.ChannelId == 0)
				throw new ArgumentException ("channel must be an existing channel", "channel");

			this.context.Connection.Send (new ChannelEditMessage (channel) { Delete = true });
		}

		/// <summary>
		/// Clears the channel manager of all channels.
		/// </summary>
		public void Clear()
		{
			lock (channelLock)
			{
				this.channels = null;
				OnCollectionChanged (new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Reset));
			}
		}

		#region IEnumerable<Channel> members
		public IEnumerator<ChannelInfo> GetEnumerator ()
		{
			if (this.channels == null || this.channels.Count == 0)
				return Enumerable.Empty<ChannelInfo> ().GetEnumerator();

			lock (channelLock)
			{
				return channels.Values.ToList ().GetEnumerator();
			}
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator ()
		{
			return this.GetEnumerator ();
		}
		#endregion

		private readonly IClientContext context;

		private readonly object channelLock = new object ();
		private Dictionary<int, ChannelInfo> channels;

		internal void OnChannelListReceivedMessage (MessageReceivedEventArgs e)
		{
			var msg = (ChannelListMessage)e.Message;

			lock (channelLock)
			{
				this.channels = msg.Channels.ToDictionary (c => c.ChannelId);
			}

			OnReceivedChannelList (new ReceivedListEventArgs<ChannelInfo> (msg.Channels));
			OnCollectionChanged (new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Reset));
		}

		internal void OnChannelEditResultMessage (MessageReceivedEventArgs e)
		{
			var msg = (ChannelEditResultMessage)e.Message;

			ChannelInfo channel;
			lock (this.channelLock)
			{
				this.channels.TryGetValue (msg.ChannelId, out channel);
			}

			OnReceivedChannelEditResult (new ChannelEditResultEventArgs (channel, msg.Result));
		}

		protected internal virtual void OnReceivedChannelList (ReceivedListEventArgs<ChannelInfo> e)
		{
			var received = this.ReceivedChannelList;
			if (received != null)
				received (this, e);
		}

		protected internal virtual void OnReceivedChannelEditResult (ChannelEditResultEventArgs e)
		{
			var received = this.ReceivedChannelEditResult;
			if (received != null)
				received (this, e);
		}

		protected virtual void OnCollectionChanged (NotifyCollectionChangedEventArgs e)
		{
			var changed = this.CollectionChanged;
			if (changed != null)
				changed (this, e);
		}
	}

	#region Event Args
	public class ChannelEditResultEventArgs
		: EventArgs
	{
		public ChannelEditResultEventArgs (ChannelInfo channel, ChannelEditResult result)
		{
			this.Channel = channel;
			this.Result = result;
		}

		/// <summary>
		/// Gets the channel the edit request was for.
		/// </summary>
		public ChannelInfo Channel
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets the result of the channel edit request.
		/// </summary>
		public ChannelEditResult Result
		{
			get;
			private set;
		}
	}
	#endregion
}