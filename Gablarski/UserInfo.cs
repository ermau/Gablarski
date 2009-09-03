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

		internal UserInfo (UserInfo info)
		{
			if (info == null)
				throw new ArgumentNullException ("info");

			this.Nickname = info.Nickname;
			this.Username = info.Username;
			this.UserId = info.UserId;
			this.CurrentChannelId = info.CurrentChannelId;
		}

		internal UserInfo (string nickname, string username, int userId, int currentChannelId, bool muted)
		{
			if (nickname.IsEmpty())
				throw new ArgumentNullException ("nickname");
			if (userId < 0)
				throw new ArgumentOutOfRangeException ("userId");
			if (currentChannelId < 0)
				throw new ArgumentOutOfRangeException ("currentChannelId");

			this.Nickname = nickname;
			this.Username = (username.IsEmpty()) ? nickname : username;
			this.UserId = userId;
			this.CurrentChannelId = currentChannelId;
			this.IsMuted = muted;
		}

		internal UserInfo (IValueReader reader)
		{
			if (reader == null)
				throw new ArgumentNullException("reader");

			this.Deserialize (reader);
		}

		public virtual string Username
		{
			get;
			protected set;
		}

		public virtual string Nickname
		{
			get;
			protected set;
		}

		public virtual int UserId
		{
			get;
			protected set;
		}

		public int CurrentChannelId
		{
			get;
			set;
		}

		public bool IsMuted
		{
			get; set;
		}

		internal void Serialize (IValueWriter writer)
		{
			writer.WriteInt32 (this.UserId);
			writer.WriteString (this.Username);
			writer.WriteInt32 (this.CurrentChannelId);
			writer.WriteString (this.Nickname);
			writer.WriteBool (this.IsMuted);
		}

		internal void Deserialize (IValueReader reader)
		{
			this.UserId = reader.ReadInt32();
			this.Username = reader.ReadString();
			this.CurrentChannelId = reader.ReadInt32();
			this.Nickname = reader.ReadString();
			this.IsMuted = reader.ReadBool();
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