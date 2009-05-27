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

		public ChannelChangeInfo (long targetPlayerId, long targetChannelId)
		{
			this.TargetPlayerId = TargetPlayerId;
			this.TargetChannelId = targetChannelId;
		}

		public ChannelChangeInfo (long targetPlayerId, long targetChannelId, long requestingPlayerId)
			: this (targetPlayerId, targetChannelId)
		{
			this.RequestingPlayerId = requestingPlayerId;
		}

		public ChannelChangeInfo (IValueReader reader)
		{
			this.Deserialize (reader);
		}

		/// <summary>
		/// Gets the ID of the player who moved the target player.
		/// </summary>
		public long RequestingPlayerId
		{
			get;
			set;
		}

		/// <summary>
		/// Gets the ID of the player being moved.
		/// </summary>
		public long TargetPlayerId
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
			writer.WriteInt64 (this.RequestingPlayerId);
			writer.WriteInt64 (this.TargetPlayerId);
			writer.WriteInt64 (this.TargetChannelId);
		}

		internal void Deserialize (IValueReader reader)
		{
			this.RequestingPlayerId = reader.ReadInt64 ();
			this.TargetPlayerId = reader.ReadInt64 ();
			this.TargetChannelId = reader.ReadInt64 ();
		}
	}
}
