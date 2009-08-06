using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gablarski.Messages
{
	public class ChannelListMessage
		: ServerMessage
	{
		public ChannelListMessage ()
			: base (ServerMessageType.ChannelListReceived)
		{

		}

		public ChannelListMessage (IEnumerable<ChannelInfo> channels)
			: this()
		{
			this.Channels = channels;
			this.Result = GenericResult.Success;
		}

		public ChannelListMessage (GenericResult result)
			: this ()
		{
			this.Result = result;
		}

		/// <summary>
		/// Gets or sets the result of the request.
		/// </summary>
		public GenericResult Result
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the channels in the message, <c>null</c> if request failed.
		/// </summary>
		public IEnumerable<ChannelInfo> Channels
		{
			get;
			set;
		}

		public override void WritePayload (IValueWriter writer)
		{
			writer.WriteGenericResult (this.Result);
			if (this.Result != GenericResult.Success)
				return;

			writer.WriteInt32 (this.Channels.Count ());
			foreach (var c in this.Channels)
				c.Serialize (writer);
		}

		public override void ReadPayload (IValueReader reader)
		{
			this.Result = reader.ReadGenericResult ();
			if (this.Result != GenericResult.Success)
				return;

			ChannelInfo[] channels = new ChannelInfo[reader.ReadInt32 ()];
			for (int i = 0; i < channels.Length; ++i)
				channels[i] = new ChannelInfo (reader);

			this.Channels = channels;
		}
	}
}