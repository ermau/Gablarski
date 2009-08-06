using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gablarski.Audio;

namespace Gablarski.Messages
{
	public class SourcesRemovedMessage
		: ServerMessage
	{
		public SourcesRemovedMessage ()
			: base (ServerMessageType.SourcesRemoved)
		{
		}

		public SourcesRemovedMessage(IEnumerable<AudioSource> sources)
			: this()
		{
			this.SourceIds = sources.Select (s => s.Id);
		}

		public IEnumerable<int> SourceIds
		{
			get;
			set;
		}

		public override void WritePayload (IValueWriter writer)
		{
			int count = this.SourceIds.Count();
			writer.WriteInt32 (count);
			using (var iter = this.SourceIds.GetEnumerator())
			{
				for (int i = 0; i < count && iter.MoveNext(); ++i)
					writer.WriteInt32 (iter.Current);
			}
		}

		public override void ReadPayload (IValueReader reader)
		{
			int[] sourceIds = new int[reader.ReadInt32()];
			for (int i = 0; i < sourceIds.Length; ++i)
				sourceIds[i] = reader.ReadInt32();

			this.SourceIds = sourceIds;
		}
	}
}