using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gablarski.Messages
{
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
		FailedChannelDoesntExist = 5
	}

	public class ChannelEditResultMessage
		: ServerMessage
	{
		public ChannelEditResultMessage ()
			: base (ServerMessageType.ChannelEditResult)
		{
		}

		public ChannelEditResultMessage (Channel channel, ChannelEditResult result)
			: this ()
		{
			this.ChannelId = channel.ChannelId;
			this.Result = result;
		}

		public object ChannelId
		{
			get;
			set;
		}

		public ChannelEditResult Result
		{
			get;
			set;
		}

		public override void WritePayload (IValueWriter writer, IdentifyingTypes idTypes)
		{
			idTypes.WriteChannel (writer, this.ChannelId);
			writer.WriteByte ((byte)this.Result);
		}

		public override void ReadPayload (IValueReader reader, IdentifyingTypes idTypes)
		{
			this.ChannelId = idTypes.ReadChannel (reader);
			this.Result = (ChannelEditResult)reader.ReadByte();
		}
	}
}