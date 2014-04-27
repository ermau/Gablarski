// Copyright (c) 2011-2013, Eric Maupin
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
using FragLabs.Audio.Codecs;
using FragLabs.Audio.Codecs.Opus;

namespace Gablarski.Audio
{
	public sealed class AudioCodec
	{
		private readonly object codecLock = new object();

		private OpusEncoder encoder;
		private OpusDecoder decoder;

		public AudioCodec (AudioCodecArgs args)
		{
			if (args == null)
				throw new ArgumentNullException ("args");

			this.settings = args;
		}

		public AudioCodec (AudioFormat format, int bitrate, short frameSize, byte complexity)
		{
			if (format == null)
				throw new ArgumentNullException ("format");

			this.settings = new AudioCodecArgs (format, bitrate, frameSize, complexity);
		}

		public byte[] Encode (byte[] data)
		{
			if (data == null)
				throw new ArgumentNullException("data");

			if (this.encoder == null) {
				lock (this.codecLock) {
					if (this.encoder == null) {
						this.encoder = OpusEncoder.Create (this.settings.SampleRate, this.settings.Channels, Application.Voip);
						this.encoder.Bitrate = this.settings.Bitrate;
					}
				}
			}

			int encodedLength;
			byte[] encoded = this.encoder.Encode (data, data.Length, out encodedLength);
			if (encoded.Length != encodedLength) {
				byte[] final = new byte[encodedLength];
				Buffer.BlockCopy (encoded, 0, final, 0, encodedLength);
				encoded = final;
			}

			return encoded;
		}

		public byte[] Decode (byte[] data, int length)
		{
			if (this.decoder == null) {
				lock (this.codecLock) {
					if (this.decoder == null) {
						this.decoder = OpusDecoder.Create (this.settings.SampleRate, this.settings.Channels);
					}
				}
			}

			int decodedLength;
			byte[] decoded = this.decoder.Decode (data, length, out decodedLength);
			if (decoded.Length != decodedLength) {
				byte[] final = new byte[decodedLength];
				Buffer.BlockCopy (decoded, 0, final, 0, decodedLength);
				decoded = final;
			}

			return decoded;
		}

		private readonly AudioCodecArgs settings;
	}
}