using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gablarski.Media.Codecs
{
	public class RawCodec
		: IAudioCodec
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

		public uint MinSampleRate
		{
			get { return 1; }
		}

		public uint MaxSampleRate
		{
			get { return 96000; }
		}

		public uint MaxQuality
		{
			get { return 1; }
		}

		public uint MinQuality
		{
			get { return 1; }
		}

		public byte[] Encode (byte[] data, uint sampleRate, uint quality)
		{
			return data;
		}

		public byte[] Decode (byte[] encoded, uint sampleRate, uint quality)
		{
			return encoded;
		}

		#endregion
	}
}