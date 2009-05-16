﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gablarski.Messages
{
	public class PlayerDisconnectedMessage
		: ServerMessage
	{
		public PlayerDisconnectedMessage()
			: base (ServerMessageType.PlayerDisconnected)
		{
			
		}

		public PlayerDisconnectedMessage (long playerId)
			: this()
		{
			this.PlayerId = playerId;
		}

		public long PlayerId
		{
			get;
			private set;
		}

		public override void WritePayload (IValueWriter writer)
		{
			writer.WriteInt64 (this.PlayerId);
		}

		public override void ReadPayload (IValueReader reader)
		{
			this.PlayerId = reader.ReadInt64();
		}
	}
}