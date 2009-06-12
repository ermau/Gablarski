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

		public ChannelChangeInfo (object targetUserId, object targetChannelId)
		{
			this.TargetUserId = targetUserId;
			this.TargetChannelId = targetChannelId;
		}

		public ChannelChangeInfo (object targetUserId, object targetChannelId, object requestingUserId)
			: this (targetUserId, targetChannelId)
		{
			this.RequestingUserId = requestingUserId;
		}

		public ChannelChangeInfo (IValueReader reader, IdentifyingTypes idTypes)
		{
			this.Deserialize (reader, idTypes);
		}

		/// <summary>
		/// Gets the ID of the player who moved the target player.
		/// </summary>
		public object RequestingUserId
		{
			get;
			set;
		}

		/// <summary>
		/// Gets the ID of the player being moved.
		/// </summary>
		public object TargetUserId
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets the ID of the channel the player is being moved to.
		/// </summary>
		public object TargetChannelId
		{
			get;
			private set;
		}

		internal void Serialize (IValueWriter writer, IdentifyingTypes idTypes)
		{
			idTypes.WriteUser (writer, this.RequestingUserId);
			idTypes.WriteUser (writer, this.TargetUserId);
			idTypes.WriteChannel (writer, this.TargetChannelId);
		}

		internal void Deserialize (IValueReader reader, IdentifyingTypes idTypes)
		{
			this.RequestingUserId = idTypes.ReadUser (reader);
			this.TargetUserId = idTypes.ReadUser (reader);
			this.TargetChannelId = idTypes.ReadChannel (reader);
		}
	}
}
