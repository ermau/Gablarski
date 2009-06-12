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

		public MediaSourceInfo (MediaSourceBase source)
		{
			this.SourceTypeName = source.GetType().AssemblyQualifiedName;
			this.SourceId = source.Id;
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

		public object UserId
		{
			get;
			set;
		}

		internal void Serialize (IValueWriter writer, IdentifyingTypes idTypes)
		{
			writer.WriteInt32 (this.SourceId);
			writer.WriteString (this.SourceTypeName);
			idTypes.WriteUser (writer, this.UserId);
		}

		internal void Deserialize (IValueReader reader, IdentifyingTypes idTypes)
		{
			this.SourceId = reader.ReadInt32();
			this.SourceTypeName = reader.ReadString();
			this.UserId = idTypes.ReadUser (reader);
		}
	}
}