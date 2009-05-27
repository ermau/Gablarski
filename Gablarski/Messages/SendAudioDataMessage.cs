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

		public SendAudioDataMessage (long targetChannelId, int sourceId, byte[] data)
			: base (ClientMessageType.AudioData)
		{
			this.TargetChannelId = targetChannelId;
			this.SourceId = sourceId;
			this.Data = data;
		}

		public long TargetChannelId
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

		public override void WritePayload (IValueWriter writer)
		{
			writer.WriteInt64 (this.TargetChannelId);
			writer.WriteInt32 (this.SourceId);
			writer.WriteBytes (this.Data);
		}

		public override void ReadPayload (IValueReader reader)
		{
			this.TargetChannelId = reader.ReadInt64 ();
			this.SourceId = reader.ReadInt32();
			this.Data = reader.ReadBytes();
		}
	}
}