using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gablarski
{
	public class UserInfo
	{
		internal UserInfo()
		{
		}

		internal UserInfo (string nickname, long userId, long currentChannelId)
		{
			if (nickname.IsEmpty())
				throw new ArgumentNullException ("nickname");

			this.Nickname = nickname;
			this.UserId = userId;
			this.CurrentChannelId = currentChannelId;
		}

		internal UserInfo (IValueReader reader)
		{
			this.Deserialize (reader);
		}

		public string Nickname
		{
			get;
			private set;
		}

		public long UserId
		{
			get;
			private set;
		}

		public long CurrentChannelId
		{
			get;
			set;
		}

		internal void Serialize (IValueWriter writer)
		{
			writer.WriteInt64 (this.UserId);
			writer.WriteInt64 (this.CurrentChannelId);
			writer.WriteString (this.Nickname);
		}

		internal void Deserialize (IValueReader reader)
		{
			this.UserId = reader.ReadInt64();
			this.CurrentChannelId = reader.ReadInt64 ();
			this.Nickname = reader.ReadString();			
		}
	}
}