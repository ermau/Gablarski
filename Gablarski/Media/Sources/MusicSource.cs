using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gablarski.Media.Codecs;
using Gablarski.Media.Sources;

namespace Gablarski.Media.Sources
{
	public class MusicSource
		: MediaSourceBase
	{
		public MusicSource (int sourceId, object ownerId)
			: base (MediaType.Music, sourceId, ownerId)
		{
		}

		#region MediaSourceBase Members

		public override IAudioCodec AudioCodec
		{
			get { throw new NotImplementedException (); }
		}

		public override IMediaCodec VideoCodec
		{
			get { throw new NotSupportedException (); }
		}

		#endregion
	}
}