using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gablarski.Codecs;

namespace Gablarski
{
	/// <summary>
	/// Voice media source implementation
	/// </summary>
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
			get; private set;
		}

		public MediaSourceType Type
		{
			get { return MediaSourceType.Voice; }
		}

		public IMediaCodec Codec
		{
			get { return this.codec; }
		}
		#endregion

		private readonly IMediaCodec codec = new PassthruCodec();
	}
}