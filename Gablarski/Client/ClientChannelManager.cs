using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gablarski.Messages;

namespace Gablarski.Client
{
	public class ClientChannelManager
		: IEnumerable<Channel>
	{
		internal ClientChannelManager (IClientConnection connection)
		{
			this.connection = connection;
		}

		#region Events
		/// <summary>
		/// The result of a channel edit request has been received.
		/// </summary>
		public event EventHandler<ChannelEditResultEventArgs> ReceivedChannelEditResult;

		/// <summary>
		/// A new or updated player list has been received.
		/// </summary>
		public event EventHandler<ReceivedListEventArgs<Channel>> ReceivedChannelList;
		#endregion

		/// <summary>
		/// Send a create channel request to the server.
		/// </summary>
		/// <param name="channel">The channel to create.</param>
		public void Create (Channel channel)
		{
			if (channel == null)
				throw new ArgumentNullException ("channel");

			if (channel.ChannelId != 0)
				throw new ArgumentException ("Can not create an existing channel", "channel");

			this.connection.Send (new ChannelEditMessage (channel));
		}

		/// <summary>
		/// Sends an update request to the server for <paramref name="channel"/>.
		/// </summary>
		/// <param name="channel">The updated information for the channel.</param>
		public void Update (Channel channel)
		{
			if (channel == null)
				throw new ArgumentNullException ("channel");

			if (channel.ChannelId == 0)
				throw new ArgumentException ("channel must be an existing channel", "channel");

			this.connection.Send (new ChannelEditMessage (channel));
		}

		/// <summary>
		/// Sends a delete channel request to the server.
		/// </summary>
		/// <param name="channel">The channel to delete.</param>
		public void Delete (Channel channel)
		{
			if (channel == null)
				throw new ArgumentNullException ("channel");

			if (channel.ChannelId == 0)
				throw new ArgumentException ("channel must be an existing channel", "channel");

			this.connection.Send (new ChannelEditMessage (channel) { Delete = true });
		}

		public IEnumerator<Channel> GetEnumerator ()
		{
			if (this.channels == null || this.channels.Count == 0)
				return Enumerable.Empty<Channel> ().GetEnumerator();

			lock (channelLock)
			{
				return channels.Values.ToList ().GetEnumerator();
			}
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator ()
		{
			return this.GetEnumerator ();
		}

		private readonly IClientConnection connection;

		private readonly object channelLock = new object ();
		private Dictionary<long, Channel> channels;

		/// <returns><c>null</c> if no channel exists by the identifier.</returns>
		protected internal Channel this[long identifier]
		{
			get
			{
				if (this.channels == null || this.channels.Count == 0)
					return null;

				lock (channelLock)
				{
					if (!this.channels.ContainsKey (identifier))
						return null;

					return this.channels[identifier];
				}
			}
		}

		internal void OnChannelListReceivedMessage (MessageReceivedEventArgs e)
		{
			var msg = (ChannelListMessage)e.Message;

			lock (channelLock)
			{
				this.channels = msg.Channels.ToDictionary (c => c.ChannelId);
			}

			OnReceivedChannelList (new ReceivedListEventArgs<Channel> (msg.Channels));
		}

		internal void OnChannelEditResultMessage (MessageReceivedEventArgs e)
		{
			var msg = (ChannelEditResultMessage)e.Message;

			Channel channel;
			lock (this.channelLock)
			{
				this.channels.TryGetValue (msg.ChannelId, out channel);

				if (channel != null && msg.Result == ChannelEditResult.Success)
					this.channels[msg.ChannelId] = channel;
			}

			OnReceivedChannelEditResult (new ChannelEditResultEventArgs (channel, msg.Result));
		}

		protected internal virtual void OnReceivedChannelList (ReceivedListEventArgs<Channel> e)
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
	}
}