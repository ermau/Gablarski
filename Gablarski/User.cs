using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lidgren.Network;
using Gablarski.Server;

namespace Gablarski
{
	public class User
	{
		protected internal User (string nickname)
		{
			this.Nickname = nickname;
		}

		internal User ()
		{
		}

		public uint ID
		{
			get;
			protected set;
		}

		public string Nickname
		{
			get;
			protected set;
		}

		public string Username
		{
			get { return (this.username ?? this.Nickname); }
		}

		public UserState State
		{
			get;
			protected set;
		}

		public ChannelInfo Channel
		{
			get;
			internal set;
		}

		private string username;

		internal void Encode (NetBuffer buffer)
		{
			buffer.WriteVariableUInt32 (this.ID);
			buffer.WriteVariableUInt32 ((uint)this.State);
			buffer.Write (this.Username);
		}

		internal User Decode (NetBuffer buffer)
		{
			this.ID = buffer.ReadVariableUInt32 ();
			this.State = (UserState)buffer.ReadVariableUInt32 ();

			if ((this.State & UserState.Registered) == UserState.Registered)
				this.username = buffer.ReadString ();
			else
				this.Nickname = buffer.ReadString ();

			return this;
		}
	}

	[Flags]
	public enum UserState
		: uint
	{
		Unregistered	= 0,
		Registered		= 1
	}
}