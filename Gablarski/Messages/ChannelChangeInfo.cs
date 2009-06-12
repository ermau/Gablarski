using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gablarski.Messages
{
	public class ChannelChangeInfo
	{
		public ChannelChangeInfo ()
		{
		}

		public ChannelChangeInfo (long targetUserId, long targetChannelId)
		{
			this.TargetUserId = targetUserId;
			this.TargetChannelId = targetChannelId;
		}

		public ChannelChangeInfo (long targetUserId, long targetChannelId, long requestingUserId)
			: this (targetUserId, targetChannelId)
		{
			this.RequestingUserId = requestingUserId;
		}

		public ChannelChangeInfo (IValueReader reader)
		{
			this.Deserialize (reader);
		}

		/// <summary>
		/// Gets the ID of the player who moved the target player.
		/// </summary>
		public long RequestingUserId
		{
			get;
			set;
		}

		/// <summary>
		/// Gets the ID of the player being moved.
		/// </summary>
		public long TargetUserId
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets the ID of the channel the player is being moved to.
		/// </summary>
		public long TargetChannelId
		{
			get;
			private set;
		}

		internal void Serialize (IValueWriter writer)
		{
			writer.WriteInt64 (this.RequestingUserId);
			writer.WriteInt64 (this.TargetUserId);
			writer.WriteInt64 (this.TargetChannelId);
		}

		internal void Deserialize (IValueReader reader)
		{
			this.RequestingUserId = reader.ReadInt64 ();
			this.TargetUserId = reader.ReadInt64 ();
			this.TargetChannelId = reader.ReadInt64 ();
		}
	}
}
