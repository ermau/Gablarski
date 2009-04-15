using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gablarski.Media.Codecs
{
	public class RawCodec
		: IMediaCodec
	{
		#region IMediaCodec Members

		public string Name
		{
			get { return "Raw"; }
		}

		public MediaTypes SupportedTypes
		{
			get { return MediaTypes.All; }
		}

		public IEnumerable<uint> Bitrates
		{
			get { return new uint[] { 705600 }; }
		}

		public uint MaxQuality
		{
			get { return 1; }
		}

		public uint MinQuality
		{
			get { return 1; }
		}

		public byte[] Encode (byte[] data, uint bitrate, uint quality)
		{
			return data;
		}

		public byte[] Decode (byte[] encoded, uint bitrate, uint quality)
		{
			return encoded;
		}

		#endregion
	}
}