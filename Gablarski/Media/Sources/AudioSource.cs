using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gablarski.CELT;

namespace Gablarski.Media.Sources
{
	public class AudioSource
	{
		public AudioSource (IValueReader reader, IdentifyingTypes idTypes)
		{
			if (reader == null)
				throw new ArgumentNullException ("reader");
			if (idTypes == null)
				throw new ArgumentNullException ("idTypes");

			this.Deserialize (reader, idTypes);
		}

		public AudioSource (int sourceId, object ownerId, byte channels, int bitrate, int frequency, short frameSize)
		{
			if (sourceId <= 0)
				throw new ArgumentOutOfRangeException ("sourceId");
			if (ownerId == null)
				throw new ArgumentNullException ("ownerId");
			if (sourceId < 0)
				throw new ArgumentOutOfRangeException ("sourceId");
			if (ownerId == null)
				throw new ArgumentNullException ("ownerId");
			if (bitrate <= 0)
				throw new ArgumentOutOfRangeException ("bitrate");

			this.Id = sourceId;
			this.OwnerId = ownerId;
			this.Bitrate = bitrate;

			CheckRanges (channels, frequency, frameSize);
			
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

		/// <summary>
		/// Gets the ID of the source.
		/// </summary>
		public int Id
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets the owner's identifier.
		/// </summary>
		public object OwnerId
		{
			get;
			private set;
		}

		/// <summary>
		/// The bitrate of the media data.
		/// </summary>
		public int Bitrate
		{
			get;
			private set;
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

		public byte[] Encode (byte[] data)
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

		public byte[] Decode (byte[] data)
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

		private readonly object codecLock = new object();
		private CeltEncoder encoder;
		private CeltDecoder decoder;
		private CeltMode mode;

		internal void Serialize (IValueWriter writer, IdentifyingTypes idTypes)
		{
			writer.WriteInt32 (this.Id);
			idTypes.WriteUser (writer, this.OwnerId);
			writer.WriteInt32 (this.Bitrate);

			CheckRanges (this.Channels, this.Frequency, this.FrameSize);
			writer.WriteByte (this.Channels);
			writer.WriteInt32 (this.Frequency);
			writer.WriteInt16 (this.FrameSize);
		}

		internal void Deserialize (IValueReader reader, IdentifyingTypes idTypes)
		{
			this.Id = reader.ReadInt32 ();
			this.OwnerId = idTypes.ReadUser (reader);
			this.Bitrate = reader.ReadInt32();

			this.Channels = reader.ReadByte();
			this.Frequency = reader.ReadInt32();
			this.FrameSize = reader.ReadInt16();
			CheckRanges (this.Channels, this.Frequency, this.FrameSize);
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
	}
}