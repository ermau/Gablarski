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
			: base (source.Id, source.OwnerId)
		{
			this.source = source;
			this.client = client;
		}

		public void SendAudioData (byte[] data, Channel targetChannel)
		{
			this.client.Send (new SendAudioDataMessage (targetChannel.ChannelId, this.Id, Encode (data)));
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
	}
}