// Copyright (c) 2011-2014, Eric Maupin
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
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using Gablarski.Messages;
using Tempest;

namespace Gablarski.Client
{
	public class ClientChannelHandler
		: IClientChannelHandler, INotifyCollectionChanged
	{
		protected internal ClientChannelHandler (IGablarskiClientContext context)
		{
			if (context == null)
				throw new ArgumentNullException ("context");

			this.context = context;

			this.context.RegisterMessageHandler<ChannelListMessage> (OnChannelListReceivedMessage);
		}

		public event NotifyCollectionChangedEventHandler CollectionChanged
		{
			add { ((INotifyCollectionChanged)this.channels.Values).CollectionChanged += value; }
			remove { ((INotifyCollectionChanged)this.channels.Values).CollectionChanged -= value; }
		}

		public int Count
		{
			get
			{
				lock (this.channelLock)
					return this.channels.Count;
			}
		}

		/// <summary>Gets the channel with id <paramref name="channelId"/></summary>
		/// <param name="channelId">The id of the channel.</param>
		/// <returns><c>null</c> if no channel exists by the identifier.</returns>
		public IChannelInfo this[int channelId]
		{
			get
			{
				if (this.channels == null || this.channels.Count == 0)
					return null;

				lock (channelLock)
				{
					IChannelInfo channel;
					this.channels.TryGetValue (channelId, out channel);
					return channel;
				}
			}
		}

		/// <summary>
		/// Send a create channel request to the server.
		/// </summary>
		/// <param name="channel">The channel to create.</param>
		public async Task<ChannelEditResult> CreateAsync (IChannelInfo channel)
		{
			if (channel == null)
				throw new ArgumentNullException ("channel");
			if (channel.ChannelId != 0)
				throw new ArgumentException ("Can not create an existing channel", "channel");

			var editMsg = new ChannelEditMessage (channel);
			var resultMessage = await this.context.Connection.SendFor<ChannelEditResultMessage> (editMsg).ConfigureAwait (false);
			return resultMessage.Result;
		}

		/// <summary>
		/// Sends an update request to the server for <paramref name="channel"/>.
		/// </summary>
		/// <param name="channel">The updated information for the channel.</param>
		public async Task<ChannelEditResult> UpdateAsync (IChannelInfo channel)
		{
			if (channel == null)
				throw new ArgumentNullException ("channel");
			if (channel.ChannelId == 0)
				throw new ArgumentException ("channel must be an existing channel", "channel");

			var editMsg = new ChannelEditMessage (channel);
			var resultMessage = await this.context.Connection.SendFor<ChannelEditResultMessage> (editMsg);
			return resultMessage.Result;
		}

		/// <summary>
		/// Sends a delete channel request to the server.
		/// </summary>
		/// <param name="channel">The channel to delete.</param>
		public async Task<ChannelEditResult> DeleteAsync (IChannelInfo channel)
		{
			if (channel == null)
				throw new ArgumentNullException ("channel");

			if (channel.ChannelId == 0)
				throw new ArgumentException ("channel must be an existing channel", "channel");

			var editMsg = new ChannelEditMessage (channel) { Delete = true };
			var resultMessage = await this.context.Connection.SendFor<ChannelEditResultMessage> (editMsg).ConfigureAwait (false);
			return resultMessage.Result;
		}

		/// <summary>
		/// Clears the channel manager of all channels.
		/// </summary>
		public void Reset()
		{
			lock (channelLock)
				this.channels.Clear();
		}

		public IEnumerator<IChannelInfo> GetEnumerator ()
		{
			if (this.channels == null || this.channels.Count == 0)
				return Enumerable.Empty<IChannelInfo> ().GetEnumerator();

			lock (channelLock)
				return channels.Values.ToList ().GetEnumerator();
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator ()
		{
			return this.GetEnumerator ();
		}

		private readonly IGablarskiClientContext context;

		private readonly object channelLock = new object ();
		private readonly ObservableDictionary<int, IChannelInfo> channels = new ObservableDictionary<int, IChannelInfo>();

		internal void OnChannelListReceivedMessage (MessageEventArgs<ChannelListMessage> e)
		{
			lock (channelLock) {
				HashSet<int> removedIds = new HashSet<int> (this.channels.Keys);
				removedIds.IntersectWith (e.Message.Channels.Select (c => c.ChannelId));

				foreach (int id in removedIds)
					this.channels.Remove (id);

				foreach (IChannelInfo channel in e.Message.Channels)
					((IDictionary<int, IChannelInfo>) this.channels)[channel.ChannelId] = channel;
			}
		}
	}
}