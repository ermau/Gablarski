using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gablarski.Media.Sources;

namespace Gablarski
{
	public class MediaSourceInfo
	{
		public MediaSourceInfo ()
		{
		}

		public MediaSourceInfo (IMediaSource source)
		{
			this.SourceTypeName = source.GetType().AssemblyQualifiedName;
			this.SourceId = source.ID;
			this.MediaType = source.Type;
		}

		internal MediaSourceInfo (IValueReader reader, IdentifyingTypes idTypes)
		{
			this.Deserialize (reader, idTypes);
		}

		public int SourceId
		{
			get;
			set;
		}

		public string SourceTypeName
		{
			get;
			set;
		}

		public MediaType MediaType
		{
			get;
			set;
		}

		public object PlayerId
		{
			get;
			set;
		}

		internal void Serialize (IValueWriter writer, IdentifyingTypes idTypes)
		{
			writer.WriteInt32 (this.SourceId);
			writer.WriteString (this.SourceTypeName);
			idTypes.WriteUser (writer, this.PlayerId);
		}

		internal void Deserialize (IValueReader reader, IdentifyingTypes idTypes)
		{
			this.SourceId = reader.ReadInt32();
			this.SourceTypeName = reader.ReadString();
			this.PlayerId = idTypes.ReadUser (reader);
		}
	}
}