using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gablarski.CELT;

namespace Gablarski.Media.Codecs
{
	public class CeltCodec
		: IAudioCodec
	{
		public CeltCodec ()
		{
			encoder = CeltEncoder.Create (44100, 1, 128);
			decoder = CeltDecoder.Create (encoder.Mode);
		}

		public uint MinSampleRate
		{
			get { return 32000; }
		}

		public uint MaxSampleRate
		{
			get { return 96000; }
		}

		public byte[] Encode (byte[] data, uint sampleRate, uint quality)
		{
			int length;
			byte[] encoded = encoder.Encode (data, 64500, out length);

			byte[] copy = new byte[length];
			Array.Copy (encoded, copy, length);

			return copy;
		}

		public byte[] Decode (byte[] encoded, uint sampleRate, uint quality)
		{
			return decoder.Decode (encoded);
		}

		public string Name
		{
			get { return "CELT"; }
		}

		public MediaTypes SupportedTypes
		{
			get { return MediaTypes.Music | MediaTypes.Voice; }
		}

		public uint MaxQuality
		{
			get { return 10; }
		}

		public uint MinQuality
		{
			get { return 0; }
		}

		private CeltEncoder encoder;
		private CeltDecoder decoder;
	}
}