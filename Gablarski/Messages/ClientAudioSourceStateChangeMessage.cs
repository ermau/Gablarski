using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gablarski.Messages
{
	public class ClientAudioSourceStateChangeMessage
		: ClientMessage
	{
		public ClientAudioSourceStateChangeMessage ()
			: base (ClientMessageType.ClientAudioSourceStateChange)
		{
		}

		public ClientAudioSourceStateChangeMessage (bool starting, int sourceId, int channelID)
			: base (ClientMessageType.ClientAudioSourceStateChange)
		{
			this.Starting = starting;
			this.SourceId = sourceId;
			this.ChannelId = channelID;
		}

		public bool Starting
		{
			get; set;
		}

		public int SourceId
		{
			get; set;
		}

		public int ChannelId
		{
			get; set;
		}

		#region Overrides of MessageBase

		public override void WritePayload(IValueWriter writer)
		{
			writer.WriteBool (this.Starting);
			writer.WriteInt32 (this.SourceId);
			writer.WriteInt32 (this.ChannelId);
		}

		public override void ReadPayload(IValueReader reader)
		{
			this.Starting = reader.ReadBool();
			this.SourceId = reader.ReadInt32();
			this.ChannelId = reader.ReadInt32();
		}

		#endregion
	}
}