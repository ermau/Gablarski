using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gablarski.CELT;
using Gablarski.Media.Sources;

namespace Gablarski.Media.Sources
{
	public class AudioSource
		: MediaSourceBase
	{
		public AudioSource (MediaSourceBase sourceBase)
			: base (sourceBase.Id, sourceBase.OwnerId, sourceBase.Bitrate)
		{
		}

		public AudioSource (IValueReader reader, IdentifyingTypes idTypes)
			: base (reader, idTypes)
		{
		}

		public AudioSource (int id, object ownerId, byte channels, int bitrate, int frequency, short frameSize)
			: base (id, ownerId, bitrate)
		{
			if (id <= 0)
				throw new ArgumentOutOfRangeException ("id");
			if (ownerId == null)
				throw new ArgumentNullException ("ownerId");
			if (channels <= 0 || channels > 2)
				throw new ArgumentOutOfRangeException ("channels");
			if (frequency < 20000 || frequency > 96000)
				throw new ArgumentOutOfRangeException ("frequency");
			if (frameSize < 64 || frameSize > 512)
				throw new ArgumentOutOfRangeException ("frameSize");
			
			this.Channels = channels;
			this.Frequency = frequency;
			this.FrameSize = frameSize;
		}

		public AudioSource (int id, object ownerId, byte channels, int bitrate, int frequency, short frameSize, byte complexity)
			: this (id, ownerId, channels, bitrate, frequency, frameSize)
		{
			if (complexity < 1 || complexity > 10)
				throw new ArgumentOutOfRangeException ("complexity");

			this.complexity = complexity;
		}

		public override byte[] Encode (byte[] data)
		{
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
			byte[] final = new byte[len];
			Array.Copy (encoded, final, len);

			return final;
		}

		public override byte[] Decode (byte[] data)
		{
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
			private set;
		}

		/// <summary>
		/// Gets the frequency of the audio.
		/// </summary>
		public int Frequency
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets the frame size for the encoded packets.
		/// </summary>
		public short FrameSize
		{
			get;
			private set;
		}

		private readonly object codecLock = new object();
		private CeltEncoder encoder;
		private CeltDecoder decoder;
		private CeltMode mode;

		protected override void Serialize (IValueWriter writer, IdentifyingTypes idTypes)
		{
			writer.WriteByte (this.Channels);
			writer.WriteInt32 (this.Frequency);
		}

		protected override void Deserialize (IValueReader reader, IdentifyingTypes idTypes)
		{
			this.Channels = reader.ReadByte();
			this.Frequency = reader.ReadInt32();
		}
	}
}