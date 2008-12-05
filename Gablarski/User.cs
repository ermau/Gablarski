using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lidgren.Network;
using Gablarski.Server;

namespace Gablarski
{
	public abstract class User
	{
		protected User (string nickname)
		{
			this.Nickname = nickname;
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
			//buffer.WriteVariableUInt32 ((this.Channel != null) ? this.Channel.ID : 0);
			buffer.WriteVariableUInt32 ((uint)this.State);
			buffer.Write (this.Username);
		}

		internal void Decode (NetBuffer buffer, GablarskiServer server)
		{
			this.ID = buffer.ReadVariableUInt32 ();
			//this.Channel = server.Channels[buffer.ReadVariableUInt32 ()];
			this.State = (UserState)buffer.ReadVariableUInt32 ();

			if ((this.State & UserState.Registered) == UserState.Registered)
				this.username = buffer.ReadString ();
			else
				this.Nickname = buffer.ReadString ();
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