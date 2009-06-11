using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gablarski.Messages
{
	public class PlayerLoggedIn
		: ServerMessage
	{
		public PlayerLoggedIn()
			: base (ServerMessageType.PlayerLoggedIn)
		{
		}

		public PlayerLoggedIn (PlayerInfo playerInfo)
			: base (ServerMessageType.PlayerLoggedIn)
		{
			this.PlayerInfo = playerInfo;
		}

		public PlayerInfo PlayerInfo
		{
			get;
			set;
		}

		public override void WritePayload (IValueWriter writer)
		{
			this.PlayerInfo.Serialize (writer);
		}

		public override void ReadPayload (IValueReader reader)
		{
			this.PlayerInfo = new PlayerInfo (reader);
		}
	}
}