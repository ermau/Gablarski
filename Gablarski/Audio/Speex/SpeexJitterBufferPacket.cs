using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Gablarski.Audio.Speex
{
	public class SpeexJitterBufferPacket
	{
		public SpeexJitterBufferPacket ()
		{
			
		}

		public SpeexJitterBufferPacket (byte[] data, uint sequence, AudioSource source)
		{
			this.Span = (uint)source.FrameSize;
			this.Encoded = true;
			this.Data = data;

			this.TimeStamp = sequence*(uint)source.FrameSize;
			this.Sequence = sequence;
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
	}
}