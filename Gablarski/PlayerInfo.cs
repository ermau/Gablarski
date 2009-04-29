using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gablarski
{
	public class PlayerInfo
	{
		internal PlayerInfo()
		{

		}

		internal PlayerInfo (string nickname, long playerId)
		{
			this.Nickname = nickname;
			this.PlayerId = playerId;
		}

		internal PlayerInfo (IValueReader reader)
		{
			this.Deserialize (reader);
		}

		public string Nickname
		{
			get;
			private set;
		}

		public long PlayerId
		{
			get;
			private set;
		}

		/// <summary>
		/// Server use.
		/// </summary>
		internal IConnection connection;

		internal void Serialize (IValueWriter writer)
		{
			writer.WriteInt64 (this.PlayerId);
			writer.WriteString (this.Nickname);
		}

		internal void Deserialize (IValueReader reader)
		{
			this.PlayerId = reader.ReadInt64();
			this.Nickname = reader.ReadString();
		}
	}
}