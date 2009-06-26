using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gablarski.Media.Sources;
using Gablarski.Messages;

namespace Gablarski.Client
{
	public class ClientMediaSource
		: MediaSourceBase
	{
		internal ClientMediaSource (MediaSourceBase source, IClientConnection client)
			: base (source.Id, source.OwnerId, source.Bitrate)
		{
			this.source = source;
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

		#region Overrides of MediaSourceBase

		public override byte[] Encode (byte[] data)
		{
			return this.source.Encode (data);
		}

		public override byte[] Decode (byte[] data)
		{
			return this.source.Decode (data);
		}

		#endregion

		private readonly IClientConnection client;
		private readonly MediaSourceBase source;

		protected override void Deserialize (IValueReader reader, IdentifyingTypes idTypes)
		{
		}

		protected override void Serialize (IValueWriter writer, IdentifyingTypes idTypes)
		{
		}
	}
}