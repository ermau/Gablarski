using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gablarski.Media.Sources;

namespace Gablarski.Messages
{
	public class SourceListMessage
		: ServerMessage
	{
		public SourceListMessage()
			: base (ServerMessageType.SourceListReceived)
		{

		}

		public SourceListMessage (IEnumerable<MediaSourceBase> sources)
			: this()
		{
			this.Sources = sources;
		}

		public IEnumerable<MediaSourceBase> Sources
		{
			get;
			set;
		}

		public override void WritePayload (IValueWriter writer, IdentifyingTypes idTypes)
		{
			writer.WriteInt32 (this.Sources.Count());
			foreach (MediaSourceBase source in this.Sources)
				source.SerializeCore (writer, idTypes);
		}

		public override void ReadPayload (IValueReader reader, IdentifyingTypes idTypes)
		{
			MediaSourceBase[] sourceInfos = new MediaSourceBase[reader.ReadInt32()];
			for (int i = 0; i < sourceInfos.Length; ++i)
				sourceInfos[i] = new AudioSource (reader, idTypes);

			this.Sources = sourceInfos;
		}
	}
}