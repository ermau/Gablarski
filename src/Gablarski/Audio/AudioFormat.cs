// Copyright (c) 2010, Eric Maupin
// All rights reserved.
//
// Redistribution and use in source and binary forms, with
// or without modification, are permitted provided that
// the following conditions are met:
//
// - Redistributions of source code must retain the above 
//   copyright notice, this list of conditions and the
//   following disclaimer.
//
// - Redistributions in binary form must reproduce the above
//   copyright notice, this list of conditions and the
//   following disclaimer in the documentation and/or other
//   materials provided with the distribution.
//
// - Neither the name of Gablarski nor the names of its
//   contributors may be used to endorse or promote products
//   or services derived from this software without specific
//   prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS
// AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED
// WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
// WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR
// PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT
// HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT,
// INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
// (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE
// GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
// INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY,
// WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
// NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
// THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH
// DAMAGE.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Gablarski.Audio
{
	public enum WaveFormatEncoding
		: ushort
	{
		Unknown = 0,
		LPCM = 1,
	}

	public class AudioFormat : IEquatable<AudioFormat>
	{
		public AudioFormat (IValueReader reader)
		{
			if (reader == null)
				throw new ArgumentNullException ("reader");

			// ReSharper disable DoNotCallOverridableMethodsInConstructor
			Deserialize (reader);
			// ReSharper restore DoNotCallOverridableMethodsInConstructor
		}

		public AudioFormat (WaveFormatEncoding waveEncoding, int channels, int bitsPerSample, int sampleRate)
		{
			WaveEncoding = waveEncoding;
			Channels = channels;
			BitsPerSample = bitsPerSample;
			SampleRate = sampleRate;
		}

		protected AudioFormat()
		{
			
		}

		public WaveFormatEncoding WaveEncoding
		{
			get;
			protected set;
		}

		public int Channels
		{
			get { return this.channels; }
			protected set
			{
				if (value > 32)
					throw new ArgumentOutOfRangeException ("value", value, "Channels can not be >32");

				this.channels = value;
			}
		}

		public int BitsPerSample
		{
			get { return this.bitsPerSample; }
			protected set
			{
				if (value > Byte.MaxValue)
					throw new ArgumentOutOfRangeException("value", value, "BitsPerSample can not be >" + Byte.MaxValue);

				this.bitsPerSample = value;
			}
		}

		public int SampleRate
		{
			get { return this.sampleRate; }
			protected set
			{
				if (value > 192000)
					throw new ArgumentOutOfRangeException ("value", value, "SampleRate can not be >192000");

				this.sampleRate = value;
			}
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals (null, obj))
				return false;
			if (ReferenceEquals (this, obj))
				return true;
			if (obj.GetType() != typeof (AudioFormat))
				return false;
			return Equals ((AudioFormat) obj);
		}

		public bool Equals (AudioFormat other)
		{
			if (ReferenceEquals (null, other))
				return false;
			if (ReferenceEquals (this, other))
				return true;
			return other.channels == this.channels && other.bitsPerSample == this.bitsPerSample && other.sampleRate == this.sampleRate && Equals (other.WaveEncoding, this.WaveEncoding);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				int result = this.channels;
				result = (result * 397) ^ this.bitsPerSample;
				result = (result * 397) ^ this.sampleRate;
				result = (result * 397) ^ this.WaveEncoding.GetHashCode();
				return result;
			}
		}

		protected internal virtual void Serialize (IValueWriter writer)
		{
			writer.WriteUInt32 ((ushort)WaveEncoding);
			writer.WriteByte ((byte)Channels);
			writer.WriteByte ((byte)BitsPerSample);
			writer.WriteInt32 (SampleRate);
		}

		protected internal virtual void Deserialize (IValueReader reader)
		{
			WaveEncoding = (WaveFormatEncoding) reader.ReadUInt32();
			Channels = reader.ReadByte();
			BitsPerSample = reader.ReadByte();
			SampleRate = reader.ReadInt32();
		}
		
		private int channels;
		private int bitsPerSample;
		private int sampleRate;

		public static bool operator == (AudioFormat left, AudioFormat right)
		{
			return Equals (left, right);
		}

		public static bool operator != (AudioFormat left, AudioFormat right)
		{
			return !Equals (left, right);
		}

		public static readonly AudioFormat Mono16bitLPCM = new AudioFormat (WaveFormatEncoding.LPCM, 1, 16, 44100);
		public static readonly AudioFormat Stereo16bitLPCM = new AudioFormat (WaveFormatEncoding.LPCM, 2, 16, 44100);
	}
}
