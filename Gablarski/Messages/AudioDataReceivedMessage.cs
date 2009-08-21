using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gablarski.Messages
{
	public class AudioDataReceivedMessage
		: ServerMessage
	{
		public AudioDataReceivedMessage ()
			: base (ServerMessageType.AudioDataReceived)
		{
		}

		public AudioDataReceivedMessage (int sourceId, int sequence, byte[] data)
			: this()
		{
			#if DEBUG
			if (sourceId <= 0)
				throw new ArgumentOutOfRangeException("sourceId");
			if (sequence < 0)
				throw new ArgumentOutOfRangeException("sequence");
			if (data == null)
				throw new ArgumentNullException("data");
			#endif

			this.SourceId = sourceId;
			this.Sequence = sequence;
			this.Data = data;
		}

		public int Sequence
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
			writer.WriteInt32 (this.SourceId);
			writer.WriteInt32 (this.Sequence);
			writer.WriteBytes (this.Data);
		}

		public override void ReadPayload (IValueReader reader)
		{
			this.SourceId = reader.ReadInt32 ();
			this.Sequence = reader.ReadInt32();
			this.Data = reader.ReadBytes ();
		}
	}
}