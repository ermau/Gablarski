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

		public ChannelChangeInfo (int targetUserId, int targetChannelId)
		{
			if (targetUserId < 0)
				throw new ArgumentOutOfRangeException ("targetUserId");
			if (targetChannelId < 0)
				throw new ArgumentOutOfRangeException ("targetChannelId");

			this.TargetUserId = targetUserId;
			this.TargetChannelId = targetChannelId;
		}

		public ChannelChangeInfo (int targetUserId, int targetChannelId, int requestingUserId)
			: this (targetUserId, targetChannelId)
		{
			if (requestingUserId < 0)
				throw new ArgumentOutOfRangeException ("requestingUserId");

			this.RequestingUserId = requestingUserId;
		}

		public ChannelChangeInfo (IValueReader reader)
		{
			if (reader == null)
				throw new ArgumentNullException("reader");

			this.Deserialize (reader);
		}

		/// <summary>
		/// Gets the ID of the player who moved the target player.
		/// </summary>
		public int RequestingUserId
		{
			get;
			set;
		}

		/// <summary>
		/// Gets the ID of the player being moved.
		/// </summary>
		public int TargetUserId
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets the ID of the channel the player is being moved to.
		/// </summary>
		public int TargetChannelId
		{
			get;
			private set;
		}

		internal void Serialize (IValueWriter writer)
		{
			writer.WriteInt32 (this.RequestingUserId);
			writer.WriteInt32 (this.TargetUserId);
			writer.WriteInt32 (this.TargetChannelId);
		}

		internal void Deserialize (IValueReader reader)
		{
			this.RequestingUserId = reader.ReadInt32();
			this.TargetUserId = reader.ReadInt32();
			this.TargetChannelId = reader.ReadInt32();
		}
	}
}
