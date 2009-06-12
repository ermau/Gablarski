using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gablarski.Media.Codecs;

namespace Gablarski.Media.Sources
{
	public class VoiceSource
		: MediaSourceBase
	{
		public VoiceSource (int sourceId, object ownerId)
			: base (MediaType.Voice, sourceId, ownerId)
		{
		}

		#region MediaSourceBase Members

		public override IAudioCodec AudioCodec
		{
			get { return this.codec; }
		}

		public override IMediaCodec VideoCodec
		{
			get { throw new NotSupportedException (); }
		}

		#endregion

		private readonly CeltCodec codec = new CeltCodec();
	}
}