using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gablarski.Audio.Speex
{
	public class SpeexJitterBufferPacket
	{
		internal SpeexJitterBufferPacket (SpeexJitterBuffer.JitterBufferPacket nativePacket)
		{
			this.Data = Data;
			this.TimeStamp = nativePacket.timestamp;
			this.Span = nativePacket.span;
			this.Sequence = nativePacket.sequence;
		}

		public byte[] Data
		{
			get; set;
		}

		public uint TimeStamp
		{
			get; set;
		}

		public uint Span
		{
			get; set;
		}

		public uint Sequence
		{
			get; set;
		}

		internal SpeexJitterBuffer.JitterBufferPacket ToNativePacket()
		{
			return new SpeexJitterBuffer.JitterBufferPacket
			{
				data = Data,
				len = (uint)Data.Length,
				sequence = Sequence,
				span = Span,
				timestamp = TimeStamp
			};
		}
	}
}