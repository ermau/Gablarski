﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gablarski.Messages
{
	public enum ChannelChangeResult
		: byte
	{
		FailedUnknown = 0,

		Success = 1,

		FailedPermissions = 2,

		FailedUnknownChannel = 3
	}

	public class ChannelChangeResultMessage
		: ServerMessage
	{
		public ChannelChangeResultMessage ()
			: base (ServerMessageType.ChangeChannelResult)
		{
		}

		public ChannelChangeResultMessage (ChannelChangeInfo moveInfo)
			: this ()
		{
			this.Result = ChannelChangeResult.Success;
			this.MoveInfo = moveInfo;
		}

		public ChannelChangeResult Result
		{
			get;
			set;
		}

		public ChannelChangeInfo MoveInfo
		{
			get;
			set;
		}

		public override void WritePayload (IValueWriter writer, IdentifyingTypes idTypes)
		{
			writer.WriteByte ((byte)this.Result);
			
			if (this.Result == ChannelChangeResult.Success)
				this.MoveInfo.Serialize (writer, idTypes);
		}

		public override void ReadPayload (IValueReader reader, IdentifyingTypes idTypes)
		{
			this.Result = (ChannelChangeResult)reader.ReadByte ();

			if (this.Result == ChannelChangeResult.Success)
				this.MoveInfo = new ChannelChangeInfo (reader, idTypes);
		}
	}
}