using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lidgren.Network;
using Gablarski.Server;

namespace Gablarski
{
	public interface IUser
	{
		uint ID { get; }
		string Nickname { get; }
		string Username { get; }
		UserState State { get; }
		ChannelInfo Channel { get; }
	}

	internal static class UserExtensions
	{
		internal static void Encode (this IUser self, NetBuffer buffer)
		{
			buffer.WriteVariableUInt32 (self.ID);
			buffer.WriteVariableUInt32 ((uint)self.State);
			buffer.Write (self.Username);
		}

		internal static DecodedUser Decode (this IUser self, NetBuffer buffer)
		{
			var user = new DecodedUser();
			user.ID = buffer.ReadVariableUInt32 ();
			user.State = (UserState)buffer.ReadVariableUInt32 ();

			if ((user.State & UserState.Registered) == UserState.Registered)
				user.Username = buffer.ReadString ();
			else
				user.Nickname = buffer.ReadString ();

			return user;
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