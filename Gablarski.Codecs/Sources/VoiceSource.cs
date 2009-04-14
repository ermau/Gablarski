using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gablarski.Media.Codecs;

namespace Gablarski.Media.Sources
{
	public class VoiceSource
		: IMediaSource
	{
		public VoiceSource (int sourceID)
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
			get { return MediaType.Voice; }
		}

		public IMediaCodec AudioCodec
		{
			get { return this.codec; }
		}

		public IMediaCodec VideoCodec
		{
			get { throw new NotSupportedException (); }
		}

		#endregion

		private IMediaCodec codec;
	}
}