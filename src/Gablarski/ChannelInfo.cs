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
using Tempest;

namespace Gablarski
{
	public class ChannelInfo
		: IChannelInfo
	{
		public ChannelInfo ()
			: this (0)
		{
		}

		internal ChannelInfo (ISerializationContext context, IValueReader reader)
		{
			if (reader == null)
				throw new ArgumentNullException ("reader");

			Deserialize (context, reader);
		}

		public ChannelInfo (int channelId)
		{
			this.ChannelId = channelId;
		}

		public ChannelInfo (IChannelInfo channelInfo)
		{
			if (channelInfo == null)
				throw new ArgumentNullException ("channelInfo");

			ChannelId = channelInfo.ChannelId;
			ParentChannelId = channelInfo.ParentChannelId;
			Name = channelInfo.Name;
			Description = channelInfo.Description;
			UserLimit = channelInfo.UserLimit;
			ReadOnly = channelInfo.ReadOnly;
		}

		public ChannelInfo (int channelId, IChannelInfo channelInfo)
			: this (channelId)
		{
			if (channelInfo == null)
				throw new ArgumentNullException ("channelInfo");

			ParentChannelId = channelInfo.ParentChannelId;
			Name = channelInfo.Name;
			Description = channelInfo.Description;
			UserLimit = channelInfo.UserLimit;
			ReadOnly = channelInfo.ReadOnly;
		}

		/// <summary>
		/// Gets the ID of this channel.
		/// </summary>
		public virtual int ChannelId
		{
			get;
			protected set;
		}

		/// <summary>
		/// Gets or sets the channel ID this is a subchannel of. default if a main channel.
		/// </summary>
		public virtual int ParentChannelId
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the name of the channel.
		/// </summary>
		public virtual string Name
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the description of the channel.
		/// </summary>
		public virtual string Description
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the player limit. 0 for no limit.
		/// </summary>
		public virtual int UserLimit
		{
			get;
			set;
		}

		/// <summary>
		/// Gets whether this individual channel can be modified or not.
		/// </summary>
		public virtual bool ReadOnly
		{
			get;
			set;
		}

		public void Serialize (ISerializationContext context, IValueWriter writer)
		{
			writer.WriteInt32 (this.ChannelId);
			writer.WriteInt32 (this.ParentChannelId);
			writer.WriteBool (this.ReadOnly);
			writer.WriteInt32 (this.UserLimit);
			writer.WriteString (this.Name);
			writer.WriteString (this.Description);
		}

		public void Deserialize (ISerializationContext context, IValueReader reader)
		{
			this.ChannelId = reader.ReadInt32();
			this.ParentChannelId = reader.ReadInt32();
			this.ReadOnly = reader.ReadBool();
			this.UserLimit = reader.ReadInt32 ();
			this.Name = reader.ReadString ();
			this.Description = reader.ReadString ();
		}
	}

	public enum ChannelEditResult
		: byte
	{
		/// <summary>
		/// Failed for an unknown reason.
		/// </summary>
		FailedUnknown = 0,

		/// <summary>
		/// Great Success!
		/// </summary>
		Success = 1,

		/// <summary>
		/// Failed because the player does not have sufficient permissions.
		/// </summary>
		FailedPermissions = 2,

		/// <summary>
		/// Failed because no channels are updateable.
		/// </summary>
		FailedChannelsReadOnly = 3,

		/// <summary>
		/// Failed because the channel is marked as readonly.
		/// </summary>
		FailedChannelReadOnly = 4,

		/// <summary>
		/// Failed because channel doesn't exist on the server.
		/// </summary>
		FailedChannelDoesntExist = 5,

		/// <summary>
		/// Failed because you can not delete the last remaining channel.
		/// </summary>
		FailedLastChannel = 6,

		/// <summary>
		/// Failed because a channel with this name already exists.
		/// </summary>
		FailedChannelExists = 7,
	}
}