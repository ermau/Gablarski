// Copyright (c) 2011, Eric Maupin
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
using System.Linq;
using Tempest;

namespace Gablarski.Audio
{
	public class AudioCodecArgs
		: AudioFormat, IEquatable<AudioCodecArgs>
	{
		public AudioCodecArgs()
			: this (WaveFormatEncoding.LPCM, 1, 16, 48000, 44100, 512, 10)
		{
		}

		public AudioCodecArgs (AudioCodecArgs args)
		{
			if (args == null)
				throw new ArgumentNullException ("args");

			WaveEncoding = args.WaveEncoding;
			Channels = args.Channels;
			BitsPerSample = args.BitsPerSample;
			SampleRate = args.SampleRate;
			Bitrate = args.Bitrate;
			FrameSize = args.FrameSize;
			Complexity = args.Complexity;
		}

		public AudioCodecArgs (AudioFormat format, int bitrate, short frameSize, byte complexity)
		{
			if (format == null)
				throw new ArgumentNullException ("format");

			WaveEncoding = format.WaveEncoding;
			Channels = format.Channels;
			BitsPerSample = format.BitsPerSample;
			SampleRate = format.SampleRate;
			Bitrate = bitrate;
			FrameSize = frameSize;
			Complexity = complexity;
		}

		public AudioCodecArgs (WaveFormatEncoding waveEnconding, int channels, int bitsPerChannel, int frequency, int bitrate, short frameSize, byte complexity)
			: base (waveEnconding, channels, bitsPerChannel, frequency)
		{
			Bitrate = bitrate;
			FrameSize = frameSize;
			Complexity = complexity;
		}

		internal AudioCodecArgs (ISerializationContext context, IValueReader reader)
			: base (context, reader)
		{
		}

		/// <summary>
		/// The bitrate of the media data.
		/// </summary>
		public int Bitrate
		{
			get { return this.bitrate; }

			protected internal set
			{
				if (IsInvalidBitrate (value))
					throw new ArgumentOutOfRangeException ("value");

				this.bitrate = value;
			}
		}

		/// <summary>
		/// Gets the complexity of the audio encoding.
		/// </summary>
		public byte Complexity
		{
			get { return this.complexity; }

			protected internal set
			{
				if (IsInvalidComplexity (value))
					throw new ArgumentOutOfRangeException ("value");

				this.complexity = value;
			}
		}

		/// <summary>
		/// Gets the frame size for the encoded packets.
		/// </summary>
		public short FrameSize
		{
			get { return this.frameSize; }

			protected internal set
			{
				if (IsInvalidFrameSize (value))
					throw new ArgumentOutOfRangeException ("value");

				this.frameSize = value;
			}
		}

		public override bool Equals (object obj)
		{
			if (ReferenceEquals (null, obj))
				return false;
			if (ReferenceEquals (this, obj))
				return true;
			return Equals (obj as AudioCodecArgs);
		}

		public bool Equals (AudioCodecArgs other)
		{
			if (ReferenceEquals (null, other))
				return false;
			if (ReferenceEquals (this, other))
				return true;
			return base.Equals (other) && other.frameSize == this.frameSize && other.complexity == this.complexity && other.bitrate == this.bitrate;
		}

		public override int GetHashCode()
		{
			unchecked
			{
				int result = base.GetHashCode();
				result = (result * 397) ^ this.frameSize.GetHashCode();
				result = (result * 397) ^ this.complexity.GetHashCode();
				result = (result * 397) ^ this.bitrate;
				return result;
			}
		}

		private short frameSize;
		private byte complexity;
		private int bitrate;

		public override void Serialize (ISerializationContext context, IValueWriter writer)
		{
			base.Serialize (context, writer);
			writer.WriteInt32 (Bitrate);
			writer.WriteInt16 (FrameSize);
			writer.WriteByte (Complexity);
		}

		public override void Deserialize (ISerializationContext context, IValueReader reader)
		{
			base.Deserialize (context, reader);
			Bitrate = reader.ReadInt32();
			FrameSize = reader.ReadInt16();
			Complexity = reader.ReadByte();
		}

		public static bool IsInvalidFrameSize (short value)
		{
			return value < 64 || value > 1024 || (value % 64) != 0;
		}

		public static bool IsInvalidFrequency (int value)
		{
			return value < 20000 || value > 96000;
		}

		public static bool IsInvalidComplexity (byte value)
		{
			return value < 1 || value > 10;
		}

		public static bool IsInvalidBitrate (int value)
		{
			return value < 0 || value >= 320000;
		}

		public static bool operator == (AudioCodecArgs left, AudioCodecArgs right)
		{
			return Equals (left, right);
		}

		public static bool operator != (AudioCodecArgs left, AudioCodecArgs right)
		{
			return !Equals (left, right);
		}
	}
}