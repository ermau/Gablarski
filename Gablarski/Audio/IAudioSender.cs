using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gablarski.Audio
{
	public interface IAudioSender
	{
		/// <summary>
		/// Sends notifications that you're begining to send audio from <paramref name="source"/> to <paramref name="channel"/>.
		/// </summary>
		/// <param name="source">The source to send from.</param>
		/// <param name="channel">The channel to send to.</param>
		/// <exception cref="ArgumentNullException"><paramref name="source"/> is <c>null</c>.</exception>
		/// <exception cref="ArgumentException"><paramref name="source"/> does not belong to you.</exception>
		/// <exception cref="ArgumentNullException"><paramref name="channel"/> is <c>null</c>.</exception>
		void BeginSending (AudioSource source, ChannelInfo channel);

		/// <summary>
		/// Sends a frame of audio data to the source
		/// </summary>
		/// <param name="source">The source to send from.</param>
		/// <param name="channel">The channel to send to.</param>
		/// <param name="data"></param>
		/// <exception cref="ArgumentNullException"><paramref name="source"/> is <c>null</c>.</exception>
		/// <exception cref="ArgumentException"><paramref name="source"/> does not belong to you.</exception>
		/// <exception cref="ArgumentNullException"><paramref name="channel"/> is <c>null</c>.</exception>
		/// <exception cref="ArgumentNullException"><paramref name="data"/> is <c>null</c>.</exception>
		/// <exception cref="ArgumentException"><paramref name="data"/> is empty.</exception>
		void SendAudioData (AudioSource source, ChannelInfo channel, byte[] frame);

		/// <summary>
		/// Sends notifications that you're finished sending audio from <paramref name="source"/> to <paramref name="channel"/>.
		/// </summary>
		/// <param name="source">The source to send from.</param>
		/// <param name="channel">The channel to send to.</param>
		/// <exception cref="ArgumentNullException"><paramref name="source"/> is <c>null</c>.</exception>
		/// <exception cref="ArgumentException"><paramref name="source"/> does not belong to you.</exception>
		/// <exception cref="ArgumentNullException"><paramref name="channel"/> is <c>null</c>.</exception>
		void EndSending (AudioSource source, ChannelInfo channel);
	}
}