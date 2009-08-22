using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
			#endif

			this.targetChannelId = targetChannel.ChannelId;
			Interlocked.Exchange (ref this.sequence, 0);

			this.client.Send (new ClientAudioSourceStateChangeMessage (true, this.Id, this.targetChannelId));
		}

		public void EndSending ()
		{
			this.client.Send (new ClientAudioSourceStateChangeMessage (false, this.Id, this.targetChannelId));
		}

		public void SendAudioData (byte[] data)
		{
			#if DEBUG
			if (data == null)
				throw new ArgumentNullException("data");
			#endif

			this.client.Send (new SendAudioDataMessage (this.targetChannelId, this.Id, Interlocked.Increment (ref this.sequence), Encode (data)));
		}

		private int targetChannelId;
		private int sequence;
		private readonly IClientConnection client;
	}
}