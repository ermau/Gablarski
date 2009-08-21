using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gablarski.Messages
{
	public class SendAudioDataMessage
		: ClientMessage
	{
		public SendAudioDataMessage ()
			: base (ClientMessageType.AudioData)
		{
		}

		public SendAudioDataMessage (int targetChannelId, int sourceId, int sequence, byte[] data)
			: base (ClientMessageType.AudioData)
		{
			#if DEBUG
			if (sequence < 0)
				throw new ArgumentOutOfRangeException("sequence");
			if (targetChannelId <= 0)
				throw new ArgumentOutOfRangeException("targetChannelId");
			if (sourceId <= 0)
				throw new ArgumentOutOfRangeException("sourceId");
			if (data == null)
				throw new ArgumentNullException("data");
			#endif

			this.TargetChannelId = targetChannelId;
			this.SourceId = sourceId;
			this.Data = data;
		}

		public int Sequence
		{
			get;
			set;
		}

		public int TargetChannelId
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

		public override bool Reliable
		{
		    get { return false; }
		}

		public override void WritePayload (IValueWriter writer)
		{
			writer.WriteInt32 (this.TargetChannelId);
			writer.WriteInt32 (this.SourceId);
			writer.WriteInt32 (this.Sequence);
			writer.WriteBytes (this.Data);
		}

		public override void ReadPayload (IValueReader reader)
		{
			this.TargetChannelId = reader.ReadInt32();
			this.SourceId = reader.ReadInt32();
			this.Sequence = reader.ReadInt32();
			this.Data = reader.ReadBytes();
		}
	}
}