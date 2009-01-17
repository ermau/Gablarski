using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lidgren.Network;

namespace Gablarski
{
	public static class BufferExtensions
	{
		public static void Write (this NetBuffer self, IMediaSource source)
		{
			self.Write (source.GetType().FullName);
			self.WriteVariableInt32 ((int)source.Type);
		}

		public static IMediaSource ReadSource (this NetBuffer self, IUser owner)
		{
			return Sources.Create (Type.GetType (self.ReadString()), self.ReadVariableInt32(), owner);
		}

		public static void Write (this NetBuffer self, IUser user)
		{
			self.WriteVariableUInt32(user.ID);
			//buffer.WriteVariableUInt32 ((uint)self.State);
			self.Write(user.Nickname);
			self.Write(user.Username);
		}

		public static IUser ReadUser (this NetBuffer self)
		{
			return new DecodedUser
			{
				ID = self.ReadVariableUInt32(),
				//State = ((UserState) buffer.ReadVariableUInt32()),
				Nickname = self.ReadString(),
				Username = self.ReadString()
			};
		}
	}
}