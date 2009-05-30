using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gablarski.Media.Codecs
{
	public class CeltCodec
		: IAudioCodec
	{
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
			throw new NotImplementedException ();
		}

		public byte[] Decode (byte[] encoded, uint sampleRate, uint quality)
		{
			throw new NotImplementedException ();
		}

		public string Name
		{
			get { throw new NotImplementedException (); }
		}

		public MediaTypes SupportedTypes
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
	}
}