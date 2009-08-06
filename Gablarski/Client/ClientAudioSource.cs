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

		public void SendAudioData (byte[] data, ChannelInfo targetChannel)
		{
			SendAudioData (data, targetChannel.ChannelId);
		}

		public void SendAudioData (byte[] data, int targetChannelId)
		{
			this.client.Send (new SendAudioDataMessage (targetChannelId, this.Id, Encode (data)));
		}

		private readonly IClientConnection client;
	}
}