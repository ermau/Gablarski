using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gablarski.Codecs
{
	public class RawCodec
		: IMediaCodec
	{
		#region IMediaCodec Members

		public string Name
		{
			get { throw new NotImplementedException (); }
		}

		public MediaTypes SupportedTypes
		{
			get { throw new NotImplementedException (); }
		}

		public IEnumerable<uint> Bitrates
		{
			get { throw new NotImplementedException (); }
		}

		public uint MaxQuality
		{
			get { throw new NotImplementedException (); }
		}

		public uint MinQuality
		{
			get { throw new NotImplementedException (); }
		}

		public void Encode (byte[] data, uint bitrate, uint quality)
		{
			throw new NotImplementedException ();
		}

		public byte[] Decode (byte[] encoded, uint bitrate, uint quality)
		{
			throw new NotImplementedException ();
		}

		#endregion
	}
}