using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gablarski
{
	public class Channel
	{
		public Channel ()
			: this (null)
		{
		}

		public Channel (IValueReader reader, IdentifyingTypes idTypes)
		{
			Deserialize (reader, idTypes);
		}

		public Channel (object channelId)
			: this (channelId, false)
		{
			this.ChannelId = channelId;
		}

		public Channel (object channelId, bool readOnly)
		{
			this.ReadOnly = readOnly;
			this.ChannelId = channelId;
		}

		public Channel (object channelId, Channel channel)
			: this (channelId, channel.ReadOnly)
		{
			this.ParentChannelId = channel.ParentChannelId;
			this.Name = channel.Name;
			this.Description = channel.Description;
			this.PlayerLimit = channel.PlayerLimit;
		}

		/// <summary>
		/// Gets the ID of this channel.
		/// </summary>
		public object ChannelId
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets or sets the channel ID this is a subchannel of. 0 if a main channel.
		/// </summary>
		public object ParentChannelId
		{
			get;
			set;
		}

		/// <summary>
		/// Gets the name of the channel.
		/// </summary>
		public string Name
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
		/// Gets the description of the channel.
		/// </summary>
		public string Description
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
		/// Gets the player limit. 0 for no limit.
		/// </summary>
		public int PlayerLimit
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
		public bool ReadOnly
		{
			get;
			private set;
		}

		private string name;
		private string description;
		private int playerLimit;

		internal void Serialize (IValueWriter writer, IdentifyingTypes idTypes)
		{
			idTypes.WriteChannel (writer, this.ChannelId);
			idTypes.WriteChannel (writer, this.ParentChannelId);
			writer.WriteInt32 (this.PlayerLimit);
			writer.WriteString (this.Name);
			writer.WriteString (this.Description);
		}

		internal void Deserialize (IValueReader reader, IdentifyingTypes idTypes)
		{
			this.ChannelId = idTypes.ReadChannel (reader);
			this.ParentChannelId = idTypes.ReadChannel (reader);
			this.PlayerLimit = reader.ReadInt32 ();
			this.Name = reader.ReadString ();
			this.Description = reader.ReadString ();
		}
	}
}