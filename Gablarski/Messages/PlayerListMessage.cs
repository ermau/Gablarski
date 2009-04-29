using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gablarski.Messages
{
	public class PlayerListMessage
		: ServerMessage
	{
		public PlayerListMessage()
			: base (ServerMessageType.PlayerListReceived)
		{
		}

		public PlayerListMessage (IEnumerable<PlayerInfo> players)
			: this()
		{
			this.Players = players;
		}

		public IEnumerable<PlayerInfo> Players
		{
			get;
			set;
		}

		public override void WritePayload (IValueWriter writer)
		{
			writer.WriteInt32 (this.Players.Count());
			foreach (PlayerInfo info in this.Players)
				info.Serialize (writer);
		}

		public override void ReadPayload (IValueReader reader)
		{
			PlayerInfo[] players = new PlayerInfo[reader.ReadInt32()];
			for (int i = 0; i < players.Length; ++i)
				players[i] = new PlayerInfo (reader);

			this.Players = players;
		}
	}
}