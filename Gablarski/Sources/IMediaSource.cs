using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gablarski.Media.Codecs;

namespace Gablarski.Media.Sources
{
	public interface IMediaSource
	{
		/// <summary>
		/// Gets the ID of the source.
		/// </summary>
		int ID { get; }

		/// <summary>
		/// Gets the type of the source.
		/// </summary>
		MediaType Type { get; }

		IAudioCodec AudioCodec { get; }
		IMediaCodec VideoCodec { get; }
	}
}