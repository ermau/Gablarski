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

		internal MediaSourceInfo (IValueReader reader)
		{
			this.Deserialize (reader);
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

		public long PlayerId
		{
			get;
			set;
		}

		internal void Serialize (IValueWriter writer)
		{
			writer.WriteInt32 (this.SourceId);
			writer.WriteString (this.SourceTypeName);
			writer.WriteInt64 (this.PlayerId);
		}

		internal void Deserialize (IValueReader reader)
		{
			this.SourceId = reader.ReadInt32();
			this.SourceTypeName = reader.ReadString();
			this.PlayerId = reader.ReadInt64();
		}
	}
}