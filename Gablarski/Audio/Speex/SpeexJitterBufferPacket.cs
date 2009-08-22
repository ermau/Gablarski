using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gablarski.Audio.Speex
{
	public class SpeexJitterBufferPacket
	{
		public SpeexJitterBufferPacket (byte[] data, uint sequence, AudioSource source)
		{
			this.Span = (uint)source.FrameSize;
			this.Encoded = true;
			this.Data = data;
			this.TimeStamp = sequence*(uint)source.FrameSize;
			this.Sequence = sequence;
		}

		internal SpeexJitterBufferPacket (SpeexJitterBuffer.JitterBufferPacket nativePacket, bool encoded)
		{
			this.Encoded = encoded;
			this.Data = Data;
			this.TimeStamp = nativePacket.timestamp;
			this.Span = nativePacket.span;
			this.Sequence = nativePacket.sequence;
		}

		public bool Encoded
		{
			get; set;
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