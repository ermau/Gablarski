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

		/// <summary>
		/// Gets a listing of supported bitrates
		/// </summary>
		IEnumerable<uint> Bitrates { get; }

		/// <summary>
		/// Gets the maximum quality setting.
		/// </summary>
		uint MaxQuality { get; }

		/// <summary>
		/// Gets the minimum quality setting.
		/// </summary>
		uint MinQuality { get; }

		/// <summary>
		/// Encodes the supplied <paramref name="data"/> with the given <paramref name="bitrate"/> and <paramref name="quality"/>.
		/// </summary>
		/// <param name="data">The data to be encoded.</param>
		/// <param name="bitrate">The bitrate to encode with.</param>
		/// <param name="quality">The quality setting to encode with.</param>
		void Encode (byte[] data, uint bitrate, uint quality);

		/// <summary>
		/// Decodes the supplied <paramref name="encoded"/> data with the known <paramref name="bitrate"/> and <paramref name="quality"/>.
		/// </summary>
		/// <param name="encoded">The encoded data to be decoded.</param>
		/// <param name="bitrate">The bitrate to decode with.</param>
		/// <param name="quality">The quality setting to decode with.</param>
		/// <returns></returns>
		byte[] Decode (byte[] encoded, uint bitrate, uint quality);
	}

	
}