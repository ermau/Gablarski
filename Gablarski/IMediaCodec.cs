using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gablarski
{
	/// <summary>
	/// Interface for multiple codec implementations for different media types
	/// </summary>
	public interface IMediaCodec
	{
		int MinimumSamples { get; }

		/// <summary>
		/// The supported media types of the codec
		/// </summary>
		MediaSourceType SupportedTypes { get; }

		byte[] Encode (byte[] buffer);
		byte[] Decode (byte[] encoded);
	}
}