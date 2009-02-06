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
		uint ID { get; }

		/// <summary>
		/// Gets the type of the source.
		/// </summary>
		MediaType Type { get; }
	}
}