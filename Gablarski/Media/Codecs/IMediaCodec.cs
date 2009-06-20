using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gablarski.Media.Codecs
{
	public interface IMediaCodec
	{
		/// <summary>
		/// Gets the name of the codec.
		/// </summary>
		string Name { get; }

		/// <summary>
		/// Gets the supported media types.
		/// </summary>
		MediaTypes SupportedTypes { get; }
	}
}