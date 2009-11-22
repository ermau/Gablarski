// Copyright (c) 2009, Eric Maupin
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
using System.Text;
using Gablarski.CELT;

namespace Gablarski.Audio
{
	public class AudioSource
	{
		public AudioSource (IValueReader reader)
		{
			if (reader == null)
				throw new ArgumentNullException ("reader");

			Deserialize (reader);
		}

		public AudioSource (AudioSource source)
			: this (source.Name, source.Id, source.OwnerId, source.Channels, source.Bitrate, source.Frequency, source.FrameSize, source.Complexity, source.IsMuted)
		{
		}

		public AudioSource (string name, int sourceId, int ownerId, byte channels, int bitrate, int frequency, short frameSize, byte complexity, bool muted)
		{
			if (name == null)
				throw new ArgumentNullException ("name");
			if (sourceId <= 0)
				throw new ArgumentOutOfRangeException ("sourceId");
			if (ownerId == 0)
				throw new ArgumentException ("ownerId");
			if (bitrate <= 0)
				throw new ArgumentOutOfRangeException ("bitrate");
			if (complexity < 1 || complexity > 10)
				throw new ArgumentOutOfRangeException ("complexity");

			CheckRanges (channels, frequency, frameSize);

			this.Name = name;
			this.Id = sourceId;
			this.OwnerId = ownerId;
			this.Bitrate = bitrate;
			this.complexity = complexity;
			this.IsMuted = muted;

			this.Channels = channels;
			this.Frequency = frequency;
			this.FrameSize = frameSize;
		}

		/// <summary>
		/// Gets if the source is muted by you or the server.
		/// </summary>
		public bool IsMuted
		{
			get;
			protected internal set;
		}

		/// <summary>
		/// Gets the user-local name of the source.
		/// </summary>
		public string Name
		{
			get;
			protected internal set;
		}

		/// <summary>
		/// Gets the ID of the source.
		/// </summary>
		public int Id
		{
			get;
			protected internal set;
		}

		/// <summary>
		/// Gets the owner's identifier.
		/// </summary>
		public int OwnerId
		{
			get;
			protected internal set;
		}

		/// <summary>
		/// The bitrate of the media data.
		/// </summary>
		public int Bitrate
		{
			get;
			protected internal set;
		}

		private readonly byte complexity = 10;

		/// <summary>
		/// Gets the complexity of the audio encoding.
		/// </summary>
		public byte Complexity
		{
			get { return this.complexity; }
		}

		/// <summary>
		/// Gets the number of audio channels in this source.
		/// </summary>
		public byte Channels
		{
			get;
			protected internal set;
		}

		/// <summary>
		/// Gets the frequency of the audio.
		/// </summary>
		public int Frequency
		{
			get;
			protected internal set;
		}

		/// <summary>
		/// Gets the frame size for the encoded packets.
		/// </summary>
		public short FrameSize
		{
			get;
			protected internal set;
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
						this.mode = CeltMode.Create (this.Frequency, this.Channels, this.FrameSize);

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
						this.mode = CeltMode.Create (this.Frequency, this.Channels, this.FrameSize);

					if (this.decoder == null)
						this.decoder = CeltDecoder.Create (this.mode);
				}
			}

			return this.decoder.Decode (data);
		}

		public override string ToString ()
		{
			return OwnerId + ":" + Name + ":" + Id;
		}

		private readonly object codecLock = new object();
		private CeltEncoder encoder;
		private CeltDecoder decoder;
		private CeltMode mode;

		protected internal void Serialize (IValueWriter writer)
		{
			writer.WriteString (this.Name);
			writer.WriteInt32 (this.Id);
			writer.WriteInt32 (this.OwnerId);
			writer.WriteInt32 (this.Bitrate);
			writer.WriteByte (this.Channels);
			writer.WriteInt32 (this.Frequency);
			writer.WriteInt16 (this.FrameSize);
			writer.WriteBool (this.IsMuted);
		}

		protected internal void Deserialize (IValueReader reader)
		{
			this.Name = reader.ReadString();
			this.Id = reader.ReadInt32 ();
			this.OwnerId = reader.ReadInt32();
			this.Bitrate = reader.ReadInt32();
			this.Channels = reader.ReadByte();
			this.Frequency = reader.ReadInt32();
			this.FrameSize = reader.ReadInt16();
			this.IsMuted = reader.ReadBool();
		}

		protected static void CheckRanges (byte channels, int frequency, short frameSize)
		{
			if (channels <= 0 || channels > 2)
				throw new ArgumentOutOfRangeException ("channels");
			if (frequency < 20000 || frequency > 96000)
				throw new ArgumentOutOfRangeException ("frequency");
			if (frameSize < 64 || frameSize > 512)
				throw new ArgumentOutOfRangeException ("frameSize");
		}

		public override int GetHashCode ()
		{
			return this.Id.GetHashCode();
		}

		public override bool Equals (object obj)
		{
			var s = (obj as AudioSource);
			if (s != null)
				return (this.Id == s.Id);

			return base.Equals (obj);
		}
	}
}