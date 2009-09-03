using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gablarski.Client;

namespace Gablarski
{
	public class ChannelInfo
	{
		public ChannelInfo ()
			: this (0)
		{
		}

		public ChannelInfo (IValueReader reader)
		{
			Deserialize (reader);
		}

		public ChannelInfo (int channelId)
		{
			this.ChannelId = channelId;
		}

		public ChannelInfo (int channelId, ChannelInfo channel)
			: this (channelId)
		{
			this.ParentChannelId = channel.ParentChannelId;
			this.Name = channel.Name;
			this.Description = channel.Description;
			this.PlayerLimit = channel.PlayerLimit;

			this.ReadOnly = channel.ReadOnly;
		}

		/// <summary>
		/// Gets the ID of this channel.
		/// </summary>
		public virtual int ChannelId
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets or sets the channel ID this is a subchannel of. default if a main channel.
		/// </summary>
		public virtual int ParentChannelId
		{
			get;
			set;
		}

		protected string name;
		protected string description;
		protected int playerLimit;

		/// <summary>
		/// Gets or sets the name of the channel.
		/// </summary>
		public virtual string Name
		{
			get { return this.name; }
			set
			{
				if (this.ReadOnly)
					throw new InvalidOperationException ();

				this.name = value;
			}
		}

		/// <summary>
		/// Gets or sets the description of the channel.
		/// </summary>
		public virtual string Description
		{
			get { return this.description; }
			set
			{
				if (this.ReadOnly)
					throw new InvalidOperationException ();

				this.description = value;
			}
		}

		/// <summary>
		/// Gets or sets the player limit. 0 for no limit.
		/// </summary>
		public virtual int PlayerLimit
		{
			get { return this.playerLimit; }
			set
			{
				if (this.ReadOnly)
					throw new InvalidOperationException ();

				this.playerLimit = value;
			}
		}

		/// <summary>
		/// Gets whether this individual channel can be modified or not.
		/// </summary>
		public virtual bool ReadOnly
		{
			get;
			set;
		}

		internal void Serialize (IValueWriter writer)
		{
			writer.WriteInt32 (this.ChannelId);
			writer.WriteInt32 (this.ParentChannelId);
			writer.WriteBool (this.ReadOnly);
			writer.WriteInt32 (this.PlayerLimit);
			writer.WriteString (this.Name);
			writer.WriteString (this.Description);
		}

		internal void Deserialize (IValueReader reader)
		{
			this.ChannelId = reader.ReadInt32();
			this.ParentChannelId = reader.ReadInt32();
			this.ReadOnly = reader.ReadBool();
			this.playerLimit = reader.ReadInt32 ();
			this.name = reader.ReadString ();
			this.description = reader.ReadString ();
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