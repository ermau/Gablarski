// Copyright (c) 2009, Eric Maupin
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
using System.Linq;
using System.Collections.Generic;
using System.Threading;

namespace Gablarski.Server
{
	public class LobbyChannelProvider
		: IChannelProvider
	{
		public LobbyChannelProvider ()
		{
			this.lobby = new ChannelInfo (1)
			{
				Name = "Lobby",
				Description = String.Empty,
				ReadOnly = true
			};
			
			DefaultChannel = this.lobby;
		}

		public event EventHandler ChannelsUpdated;

		public Type IdentifyingType
		{
			get { return typeof (Int32); }
		}

		public bool UpdateSupported
		{
			get { return true; }
		}

		public ChannelInfo DefaultChannel
		{
			get;
			set;
		}

		public IEnumerable<ChannelInfo> GetChannels ()
		{
			yield return this.lobby;

			foreach (var c in channels.Values)
				yield return c;
		}

		public ChannelEditResult SaveChannel (ChannelInfo channel)
		{
			lock (this.channels)
			{
				if (GetChannels().Any (c => c.Name.ToLower().Trim() == channel.Name.ToLower().Trim()))
					return ChannelEditResult.FailedChannelExists;

				if (channel.ChannelId.Equals (0))
				{
					int id = Interlocked.Increment (ref this.lastId);
					channels.Add (id, new ChannelInfo (id, channel));
				}
				else if (channels.ContainsKey (channel.ChannelId))
					channels[channel.ChannelId] = channel;
				else
					return ChannelEditResult.FailedChannelDoesntExist;
			}

			OnChannelsUpdated (EventArgs.Empty);
			return ChannelEditResult.Success;
		}

		public ChannelEditResult DeleteChannel (ChannelInfo channel)
		{
			ChannelEditResult result;

			lock (this.channels)
			{
				if (!this.channels.Remove (channel.ChannelId))
					return ChannelEditResult.FailedChannelDoesntExist;
			}

			OnChannelsUpdated (EventArgs.Empty);
			return ChannelEditResult.Success;
		}

		private int lastId = 1;
		private readonly ChannelInfo lobby;
		private readonly Dictionary<object, ChannelInfo> channels = new Dictionary<object, ChannelInfo> ();

		private void OnChannelsUpdated (EventArgs e)
		{
			var changed = this.ChannelsUpdated;
			if (changed != null)
				changed (this, e);
		}
	}
}