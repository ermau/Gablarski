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

		internal UserInfo (string nickname, object userId, object currentChannelId)
		{
			if (nickname.IsEmpty())
				throw new ArgumentNullException ("nickname");
			if (userId == null)
				throw new ArgumentNullException("userId");
			if (currentChannelId == null)
				throw new ArgumentNullException("currentChannelId");

			this.Nickname = nickname;
			this.UserId = userId;
			this.CurrentChannelId = currentChannelId;
		}

		internal UserInfo (IValueReader reader, IdentifyingTypes idTypes)
		{
			if (reader == null)
				throw new ArgumentNullException("reader");
			if (idTypes == null)
				throw new ArgumentNullException("idTypes");

			this.Deserialize (reader, idTypes);
		}

		public string Nickname
		{
			get;
			protected set;
		}

		public object UserId
		{
			get;
			protected set;
		}

		public object CurrentChannelId
		{
			get;
			set;
		}

		internal void Serialize (IValueWriter writer, IdentifyingTypes idTypes)
		{
			idTypes.WriteUser (writer, this.UserId);
			idTypes.WriteChannel (writer, this.CurrentChannelId);
			writer.WriteString (this.Nickname);
		}

		internal void Deserialize (IValueReader reader, IdentifyingTypes idTypes)
		{
			this.UserId = idTypes.ReadUser (reader);
			this.CurrentChannelId = idTypes.ReadChannel (reader);
			this.Nickname = reader.ReadString();			
		}
	}
}