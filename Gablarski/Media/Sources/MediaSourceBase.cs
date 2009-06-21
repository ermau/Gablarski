using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gablarski.Media.Sources
{
	public abstract class MediaSourceBase
	{
		protected MediaSourceBase (int sourceId, object ownerId, int bitrate)
		{
			if (sourceId < 0)
				throw new ArgumentOutOfRangeException ("sourceId");
			if (ownerId == null)
				throw new ArgumentNullException ("ownerId");
			if (bitrate <= 0)
				throw new ArgumentOutOfRangeException ("bitrate");

			this.Id = sourceId;
			this.OwnerId = ownerId;
			this.Bitrate = bitrate;
		}

		protected MediaSourceBase (IValueReader reader, IdentifyingTypes idTypes)
		{
			if (reader == null)
				throw new ArgumentNullException ("reader");
			if (idTypes == null)
				throw new ArgumentNullException ("idTypes");

			this.DeserializeCore (reader, idTypes);
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

		public abstract byte[] Encode (byte[] data);
		public abstract byte[] Decode (byte[] data);

		internal void SerializeCore (IValueWriter writer, IdentifyingTypes idTypes)
		{
			writer.WriteInt32 (this.Id);
			idTypes.WriteUser (writer, this.OwnerId);
			writer.WriteInt32 (this.Bitrate);

			this.Serialize (writer, idTypes);
		}

		protected abstract void Serialize (IValueWriter writer, IdentifyingTypes idTypes);

		internal void DeserializeCore (IValueReader reader, IdentifyingTypes idTypes)
		{
			this.Id = reader.ReadInt32 ();
			this.OwnerId = idTypes.ReadUser (reader);
			this.Bitrate = reader.ReadInt32();

			this.Deserialize (reader, idTypes);
		}

		protected abstract void Deserialize (IValueReader reader, IdentifyingTypes idTypes);
	}
}