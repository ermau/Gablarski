using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gablarski.Media.Codecs
{
	public interface IAudioCodec
		: IMediaCodec
	{
		/// <summary>
		/// Gets the minimum supported sample rate for the codec.
		/// </summary>
		uint MinSampleRate { get; }

		/// <summary>
		/// Gets the maximum supported sample rate for the codec.
		/// </summary>
		uint MaxSampleRate { get; }

		/// <summary>
		/// Encodes the supplied <paramref name="data"/> with the given <paramref name="bitrate"/> and <paramref name="quality"/>.
		/// </summary>
		/// <param name="data">The data to be encoded.</param>
		/// <param name="sampleRate">The sample rate of the data.</param>
		/// <param name="quality">The quality setting to encode with.</param>
		byte[] Encode (byte[] data, uint sampleRate, uint quality);

		/// <summary>
		/// Decodes the supplied <paramref name="encoded"/> data with the known <paramref name="bitrate"/> and <paramref name="quality"/>.
		/// </summary>
		/// <param name="encoded">The encoded data to be decoded.</param>
		/// <param name="sampleRate">The sample rate of the data.</param>
		/// <param name="quality">The quality setting to decode with.</param>
		/// <returns></returns>
		byte[] Decode (byte[] encoded, uint sampleRate, uint quality);
	}
}