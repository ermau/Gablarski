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

		public AudioDataReceivedMessage (int sourceId, byte[] data)
			: this()
		{
			this.SourceId = sourceId;
			this.Data = data;
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
			writer.WriteBytes (this.Data);
		}

		public override void ReadPayload (IValueReader reader)
		{
			this.SourceId = reader.ReadInt32 ();
			this.Data = reader.ReadBytes ();
		}
	}
}