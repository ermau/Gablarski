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
using System.Collections.Generic;
using System.Linq;
using Gablarski.Messages;
using System.Threading;

namespace Gablarski.Client
{
	public class ClientChannelManager
		: IIndexedEnumerable<int, IChannelInfo>
	{
		protected internal ClientChannelManager (IClientContext context)
		{
			if (context == null)
				throw new ArgumentNullException ("context");

			this.context = context;

			this.context.RegisterMessageHandler (ServerMessageType.ChannelList, OnChannelListReceivedMessage);
			this.context.RegisterMessageHandler (ServerMessageType.ChannelEditResult, OnChannelEditResultMessage);
		}

		#region Events
		/// <summary>
		/// The result of a channel edit request has been received.
		/// </summary>
		public event EventHandler<ChannelEditResultEventArgs> ReceivedChannelEditResult;

		/// <summary>
		/// A new or updated player list has been received.
		/// </summary>
		public event EventHandler<ReceivedListEventArgs<IChannelInfo>> ReceivedChannelList;
		#endregion

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
			}
		}

		#region IEnumerable<IChannelInfo> members
		public IEnumerator<IChannelInfo> GetEnumerator ()
		{
			if (this.channels == null || this.channels.Count == 0)
				return Enumerable.Empty<IChannelInfo> ().GetEnumerator();

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
		private Dictionary<int, IChannelInfo> channels;

		internal void OnChannelListReceivedMessage (MessageReceivedEventArgs e)
		{
			var msg = (ChannelListMessage)e.Message;

			lock (channelLock)
			{
				this.channels = msg.Channels.ToDictionary (c => c.ChannelId);
			}

			OnReceivedChannelList (new ReceivedListEventArgs<IChannelInfo> (msg.Channels));
		}

		internal void OnChannelEditResultMessage (MessageReceivedEventArgs e)
		{
			var msg = (ChannelEditResultMessage)e.Message;

			IChannelInfo channel;
			lock (this.channelLock)
			{
				this.channels.TryGetValue (msg.ChannelId, out channel);
			}

			OnReceivedChannelEditResult (new ChannelEditResultEventArgs (channel, msg.Result));
		}

		protected internal virtual void OnReceivedChannelList (ReceivedListEventArgs<IChannelInfo> e)
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

	#region Event Args
	public class ChannelEditResultEventArgs
		: EventArgs
	{
		public ChannelEditResultEventArgs (IChannelInfo channel, ChannelEditResult result)
		{
			this.Channel = channel;
			this.Result = result;
		}

		/// <summary>
		/// Gets the channel the edit request was for.
		/// </summary>
		public IChannelInfo Channel
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