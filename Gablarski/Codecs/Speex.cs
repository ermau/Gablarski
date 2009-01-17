using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using org.xiph.speex;
using org.xiph.speex.spi;
using System.Runtime.InteropServices;

namespace Gablarski.Codecs
{
	public class Speex
		: IMediaCodec
	{
		#region IMediaCodec Members

		public MediaSourceType SupportedTypes
		{
			get { throw new NotImplementedException(); }
		}

		public byte[] Encode (byte[] buffer)
		{
			if (this.encoder == null)
			{
				this.encoder = new SpeexEncoder();
				this.encoder.init (2, 10, 44100, 2);
			}

			this.encoder.processData (buffer.Cast<sbyte>().ToArray(), 0, buffer.Length);

			sbyte[] encoded = new sbyte[buffer.Length];
			this.encoder.getProcessedData (encoded, 0);

			return encoded.Cast<byte>().ToArray();
		}

		public byte[] Decode (byte[] encoded)
		{
			if (this.decoder == null)
			{
				this.decoder = new SpeexDecoder();
				this.decoder.init(2, 44100, 2, false);
			}

			this.decoder.processData (encoded.Cast<sbyte>().ToArray(), 0, encoded.Length);
			sbyte[] decoded = new sbyte[encoded.Length];

			return decoded.Cast<byte>().ToArray();
		}

		#endregion

		private SpeexDecoder decoder;
		private SpeexEncoder encoder;
	}
}