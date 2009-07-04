using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gablarski.Media.Sources;
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

		public void SendAudioData (byte[] data, Channel targetChannel)
		{
			SendAudioData (data, targetChannel.ChannelId);
		}

		public void SendAudioData (byte[] data, object targetChannelId)
		{
			this.client.Send (new SendAudioDataMessage (targetChannelId, this.Id, Encode (data)));
		}

		private readonly IClientConnection client;
	}
}