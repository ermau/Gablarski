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
			buffer.Write (self.Nickname);
			buffer.Write (self.Username);
		}

		internal static DecodedUser Decode (this IUser self, NetBuffer buffer)
		{
			var user = new DecodedUser
			{
				ID = buffer.ReadVariableUInt32(),
				State = ((UserState) buffer.ReadVariableUInt32()),
				Nickname = buffer.ReadString(),
				Username = buffer.ReadString()
			};

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