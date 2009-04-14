using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gablarski
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

		IMediaCodec AudioCodec { get; }
		IMediaCodec VideoCodec { get; }
	}
}