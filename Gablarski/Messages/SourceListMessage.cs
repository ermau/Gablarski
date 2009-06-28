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

		public SourceListMessage (IEnumerable<AudioSource> sources)
			: this()
		{
			this.Sources = sources;
		}

		public IEnumerable<AudioSource> Sources
		{
			get;
			set;
		}

		public override void WritePayload (IValueWriter writer, IdentifyingTypes idTypes)
		{
			writer.WriteInt32 (this.Sources.Count());
			foreach (AudioSource source in this.Sources)
				source.Serialize (writer, idTypes);
		}

		public override void ReadPayload (IValueReader reader, IdentifyingTypes idTypes)
		{
			AudioSource[] sourceInfos = new AudioSource[reader.ReadInt32()];
			for (int i = 0; i < sourceInfos.Length; ++i)
				sourceInfos[i] = new AudioSource (reader, idTypes);

			this.Sources = sourceInfos;
		}
	}
}