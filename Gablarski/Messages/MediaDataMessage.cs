using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gablarski.Media.Sources;

namespace Gablarski.Messages
{
	public class MediaDataMessage
		: ClientMessage
	{
		public MediaDataMessage ()
			: base (ClientMessageType.MediaData)
		{
		}

		public MediaDataMessage (int sourceID, byte[] data)
			: base (ClientMessageType.MediaData)
		{

		}

		public int SourceID
		{
			get;
			set;
		}

		public byte[] MediaData
		{
			get;
			set;
		}

		public override void WritePayload (IValueWriter writer)
		{
			writer.WriteInt32 (this.SourceID);
			
		}

		public override void ReadPayload (IValueReader reader)
		{
			throw new NotImplementedException ();
		}
	}
}