using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gablarski.Messages
{
	public class RequestSourceMessage
		: ClientMessage
	{
		public RequestSourceMessage ()
			: base (ClientMessageType.RequestSource)
		{
		}

		public RequestSourceMessage (int channels, int targetBitrate)
			: this ()
		{
			if (channels < 1 || channels > 2)
				throw new ArgumentOutOfRangeException ("channels");

			this.Channels = channels;
			this.TargetBitrate = targetBitrate;
		}

		public int Channels
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the target bitrate to be requested (0 leaves it to the server.)
		/// </summary>
		public int TargetBitrate
		{
			get;
			set;
		}

		public override void WritePayload (IValueWriter writer, IdentifyingTypes idTypes)
		{
			writer.WriteByte ((byte)this.Channels);
			writer.WriteInt32 (this.TargetBitrate);
		}

		public override void ReadPayload (IValueReader reader, IdentifyingTypes idTypes)
		{
			this.Channels = reader.ReadByte ();
			this.TargetBitrate = reader.ReadInt32();
		}
	}
}