// Copyright (c) 2010, Eric Maupin
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
using Gablarski.Messages;

namespace Gablarski.Server
{
	public class ServerChannelHandler
		: IServerChannelHandler
	{
		private readonly IServerContext context;

		public ServerChannelHandler (IServerContext context)
		{
			this.context = context;
			this.context.ChannelsProvider.ChannelsUpdated += ChannelsProviderOnChannelsUpdated;
		}

		public IEnumerator<ChannelInfo> GetEnumerator()
		{
			return context.ChannelsProvider.GetChannels().GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public ChannelInfo this [int key]
		{
			get { return this.FirstOrDefault (c => c.ChannelId == key); }
		}

		private void ChannelsProviderOnChannelsUpdated (object sender, EventArgs args)
		{
			var channelIds = new HashSet<int> (context.ChannelsProvider.GetChannels().Select (c => c.ChannelId));

			foreach (UserInfo user in context.Users.Where (u => !channelIds.Contains (u.CurrentChannelId)))
				context.Users.Move (user, context.ChannelsProvider.DefaultChannel);

			this.connections.Send (new ChannelListMessage (this.channels.Values));
		}
	}
}