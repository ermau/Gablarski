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
using System.Linq;
using Gablarski.CELT;
using Tempest;

namespace Gablarski.Audio
{
	public class AudioCodec
		: AudioCodecArgs
	{
		private readonly object codecLock = new object();
		private CeltEncoder encoder;
		private CeltDecoder decoder;
		private CeltMode mode;

		public AudioCodec (AudioCodecArgs args)
			: base (args)
		{
		}

		public AudioCodec (AudioFormat format, int bitrate, short frameSize, byte complexity)
			: base (format, bitrate, frameSize, complexity)
		{
		}

		protected AudioCodec()
		{
		}

		internal AudioCodec (ISerializationContext context, IValueReader reader)
			: base (context, reader)
		{
		}

		public byte[] Encode (byte[] data)
		{
#if DEBUG
			if (data == null)
				throw new ArgumentNullException("data");
#endif

			if (this.encoder == null)
			{
				lock (this.codecLock)
				{
					if (this.mode == null)
						this.mode = CeltMode.Create (SampleRate, Channels, FrameSize);

					if (this.encoder == null)
						this.encoder = CeltEncoder.Create (this.mode);
				}
			}

			int len;
			byte[] encoded = this.encoder.Encode (data, this.Bitrate, out len);
			if (encoded.Length != len)
			{
				byte[] final = new byte[len];
				Array.Copy (encoded, final, len);
				encoded = final;
			}

			return encoded;
		}

		public byte[] Decode (byte[] data)
		{
#if DEBUG
			if (data == null)
				throw new ArgumentNullException("data");
#endif
			
			if (this.decoder == null)
			{
				lock (this.codecLock)
				{
					if (this.mode == null)
						this.mode = CeltMode.Create (SampleRate, Channels, FrameSize);

					if (this.decoder == null)
						this.decoder = CeltDecoder.Create (this.mode);
				}
			}

			return this.decoder.Decode (data);
		}
	}
}