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
			if (context == null)
				throw new ArgumentNullException ("context");

			this.context = context;
			this.context.ChannelsProvider.ChannelsUpdated += ChannelsProviderOnChannelsUpdated;
		}

		public IEnumerator<IChannelInfo> GetEnumerator()
		{
			return context.ChannelsProvider.GetChannels().GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public IChannelInfo this [int key]
		{
			get { return this.FirstOrDefault (c => c.ChannelId == key); }
		}

		private void ChannelsProviderOnChannelsUpdated (object sender, EventArgs args)
		{
			var channels = context.ChannelsProvider.GetChannels();
			var channelIds = new HashSet<int> (channels.Select (c => c.ChannelId));

			foreach (UserInfo user in context.Users.Where (u => !channelIds.Contains (u.CurrentChannelId)))
				context.Users.Move (user, context.ChannelsProvider.DefaultChannel);

			context.Connections.Send (new ChannelListMessage (channels, context.ChannelsProvider.DefaultChannel));
		}

		internal void RequestChanneListMessage (MessageReceivedEventArgs e)
		{
			if (!e.Connection.IsConnected)
				return;

			if (!context.GetPermission (PermissionName.RequestChannelList, e.Connection))
				e.Connection.Send (new ChannelListMessage (GenericResult.FailedPermissions));
			else
			{
				IEnumerable<IChannelInfo> channels = this.context.ChannelsProvider.GetChannels();
				e.Connection.Send (new ChannelListMessage (channels, context.ChannelsProvider.DefaultChannel));
			}
		}

		internal void ChannelEditMessage (MessageReceivedEventArgs e)
		{
			var msg = (ChannelEditMessage)e.Message;

			ChannelEditResult result = ChannelEditResult.FailedUnknown;

			if (msg.Channel.ChannelId != 0)
			{
				List<IChannelInfo> channels = context.ChannelsProvider.GetChannels().ToList();

				IChannelInfo realChannel = channels.FirstOrDefault (c => c.ChannelId == msg.Channel.ChannelId);

				if (realChannel == null)
					result = ChannelEditResult.FailedChannelDoesntExist;
				else if (msg.Delete && channels.Count == 1)
					result = ChannelEditResult.FailedLastChannel;
				else if (!this.context.ChannelsProvider.UpdateSupported)
					result = ChannelEditResult.FailedChannelsReadOnly;
				else if (realChannel.ReadOnly)
					result = ChannelEditResult.FailedChannelReadOnly;
				else if (msg.Channel.ChannelId != 0)
				{
					if (msg.Delete && !context.GetPermission (PermissionName.DeleteChannel, msg.Channel, e.Connection))
						result = ChannelEditResult.FailedPermissions;
					else if (!msg.Delete && !context.GetPermission (PermissionName.EditChannel, msg.Channel, e.Connection))
						result = ChannelEditResult.FailedPermissions;
				}
			}
			else if (!context.GetPermission (PermissionName.AddChannel, e.Connection))
				result = ChannelEditResult.FailedPermissions;

			if (result == ChannelEditResult.FailedUnknown)
			{
				if (!msg.Delete)
					result = this.context.ChannelsProvider.SaveChannel (msg.Channel);
				else
					result = this.context.ChannelsProvider.DeleteChannel (msg.Channel);
			}

			e.Connection.Send (new ChannelEditResultMessage (msg.Channel, result));
		}
	}
}