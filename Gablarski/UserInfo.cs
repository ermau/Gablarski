using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gablarski
{
	public class UserInfo
	{
		public UserInfo()
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

		public virtual string Nickname
		{
			get;
			protected set;
		}

		public virtual object UserId
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

		/// <summary>
		/// Gets whether <paramref name="userId"/> is the default value (basically null) or not.
		/// </summary>
		/// <param name="userId">The user identifier to check.</param>
		/// <param name="types">The <see cref="IdentifyingTypes"/> instance to check against.</param>
		/// <returns><c>true</c> if <paramref name="userId"/> is default, <c>false</c> otherwise.</returns>
		public static bool IsDefault (object userId, IdentifyingTypes types)
		{
			return userId.Equals (types.UserIdType.GetDefaultValue());
		}

		public override bool Equals (object obj)
		{
			var info = (obj as UserInfo);

			return (info != null) ? this.UserId.Equals (info.UserId) : this.UserId.Equals (obj);
		}

		public override int GetHashCode ()
		{
			return this.UserId.GetHashCode();
		}
	}
}