using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gablarski.Media.Codecs;
using Gablarski.Media.Sources;

namespace Gablarski.Sources
{
	public class MusicSource
		: IMediaSource
	{
		public MusicSource (int sourceID)
		{
			this.ID = sourceID;
		}

		#region IMediaSource Members

		public int ID
		{
			get;
			private set;
		}

		public MediaType Type
		{
			get { return MediaType.Music; }
		}

		public IMediaCodec AudioCodec
		{
			get { throw new NotImplementedException (); }
		}

		public IMediaCodec VideoCodec
		{
			get { throw new NotSupportedException (); }
		}

		#endregion
	}
}