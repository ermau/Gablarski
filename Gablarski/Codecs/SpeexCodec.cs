using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gablarski.Media.Codecs
{
	public class SpeexCodec
		: IAudioCodec
	{
		public uint MinSampleRate
		{
			get { throw new NotImplementedException (); }
		}

		public uint MaxSampleRate
		{
			get { throw new NotImplementedException (); }
		}

		public byte[] Encode (byte[] data, uint sampleRate, uint quality)
		{
			throw new NotImplementedException ();
		}

		public byte[] Decode (byte[] encoded, uint sampleRate, uint quality)
		{
			throw new NotImplementedException ();
		}

		public string Name
		{
			get { return "Speex"; }
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
	}
}