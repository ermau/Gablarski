using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gablarski.Codecs
{
	/// <summary>
	/// Codec implementation to simply pass through data
	/// </summary>
	public class PassthruCodec
		: IMediaCodec
	{
		#region IMediaCodec Members

		public int MinimumSamples
		{
			get { return 1300; }
		}

		public MediaSourceType SupportedTypes
		{
			get { return MediaSourceType.All; }
		}

		#endregion

		public byte[] Encode (byte[] buffer)
		{
			return buffer;
		}

		public byte[] Decode (byte[] encoded)
		{
			return encoded;
		}
	}
}