using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gablarski.Media.Sources;

namespace Gablarski.Messages
{
	public class SendAudioDataMessage
		: ClientMessage
	{
		public SendAudioDataMessage ()
			: base (ClientMessageType.AudioData)
		{
		}

		public SendAudioDataMessage (object targetChannelId, int sourceId, byte[] data)
			: base (ClientMessageType.AudioData)
		{
			if (targetChannelId == null)
				throw new ArgumentNullException("targetChannelId");
			if (sourceId <= 0)
				throw new ArgumentOutOfRangeException("sourceId");
			if (data == null)
				throw new ArgumentNullException("data");

			this.TargetChannelId = targetChannelId;
			this.SourceId = sourceId;
			this.Data = data;
		}

		public object TargetChannelId
		{
			get;
			set;
		}

		public int SourceId
		{
			get;
			set;
		}

		public byte[] Data
		{
			get;
			set;
		}

		//public override bool Reliable
		//{
		//    get { return false; }
		//}

		public override void WritePayload (IValueWriter writer, IdentifyingTypes idTypes)
		{
			idTypes.WriteChannel (writer, this.TargetChannelId);
			writer.WriteInt32 (this.SourceId);
			writer.WriteBytes (this.Data);
		}

		public override void ReadPayload (IValueReader reader, IdentifyingTypes idTypes)
		{
			this.TargetChannelId = idTypes.ReadChannel (reader);
			this.SourceId = reader.ReadInt32();
			this.Data = reader.ReadBytes();
		}
	}
}