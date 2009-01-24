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

		public int MinimumSamples
		{
			get { return 1300; }
		}

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

			float[] fbuffer = new float[buffer.Length / 4];
			for (int i = 0; i < fbuffer.Length; ++i)
			{
				fbuffer[i] = BitConverter.ToSingle (buffer, i);
				i += 3;
			}

			this.encoder.processData (fbuffer, fbuffer.Length);

			sbyte[] sencoded = new sbyte[this.encoder.getProcessedDataByteSize ()];
			this.encoder.getProcessedData (sencoded, 0);

			byte[] encoded = new byte[sencoded.Length];
			for (int i = 0; i < sencoded.Length; ++i)
				encoded[i] = (byte)sencoded[i];

			return encoded;
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