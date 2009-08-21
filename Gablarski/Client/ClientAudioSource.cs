using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gablarski.Audio;
using Gablarski.Messages;

namespace Gablarski.Client
{
	public class ClientAudioSource
		: AudioSource
	{
		internal ClientAudioSource (AudioSource source, IClientConnection client)
			: base (source.Name, source.Id, source.OwnerId, source.Channels, source.Bitrate, source.Frequency, source.FrameSize, source.Complexity)
		{
			this.client = client;
		}

		public void BeginSending (ChannelInfo targetChannel)
		{
			#if DEBUG
			if (targetChannel == null)
				throw new ArgumentNullException("targetChannel");

			lock (sequences)
			{
				if (sequences[targetChannel.ChannelId] != 0)
					throw new InvalidOperationException("Stop sending on a channel before begining");
			}
			#endif

			this.client.Send (new ClientAudioSourceStateChangeMessage (true, this.Id, targetChannel.ChannelId));
		}

		public void EndSending (ChannelInfo targetChannel)
		{
			#if DEBUG
			if (targetChannel == null)
				throw new ArgumentNullException ("targetChannel");
			#endif

			lock (sequences)
			{
				sequences[targetChannel.ChannelId] = 0;
			}

			this.client.Send (new ClientAudioSourceStateChangeMessage (false, this.Id, targetChannel.ChannelId));
		}

		public void SendAudioData (byte[] data, ChannelInfo targetChannel)
		{
			#if DEBUG
			if (data == null)
				throw new ArgumentNullException("data");
			if (targetChannel == null)
				throw new ArgumentNullException("targetChannel");
			#endif

			SendAudioData (data, targetChannel.ChannelId);
		}

		public void SendAudioData (byte[] data, int targetChannelId)
		{
			#if DEBUG
			if (data == null)
				throw new ArgumentNullException("data");
			#endif

			int sequence;
			lock (sequences)
			{
				sequence = sequences[targetChannelId]++;
			}

			this.client.Send (new SendAudioDataMessage (targetChannelId, this.Id, sequence, Encode (data)));
		}

		private readonly Dictionary<int, int> sequences = new Dictionary<int, int>();
		private readonly IClientConnection client;
	}
}