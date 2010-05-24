// Copyright (c) 2010, Eric Maupin
// All rights reserved.
//
// Redistribution and use in source and binary forms, with
// or without modification, are permitted provided that
// the following conditions are met:
//
// - Redistributions of source code must retain the above 
//   copyright notice, this list of conditions and the
//   following disclaimer.
//
// - Redistributions in binary form must reproduce the above
//   copyright notice, this list of conditions and the
//   following disclaimer in the documentation and/or other
//   materials provided with the distribution.
//
// - Neither the name of Gablarski nor the names of its
//   contributors may be used to endorse or promote products
//   or services derived from this software without specific
//   prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS
// AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED
// WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
// WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR
// PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT
// HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT,
// INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
// (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE
// GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
// INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY,
// WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
// NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
// THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH
// DAMAGE.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Gablarski.Messages
{
	public class SetPermissionsMessage
		: ClientMessage
	{
		public SetPermissionsMessage()
			: base (ClientMessageType.SetPermissions)
		{
		}

		public SetPermissionsMessage (IUserInfo user, IEnumerable<Permission> permissions)
			: this()
		{
			if (user == null)
				throw new ArgumentNullException ("user");
			if (permissions == null)
				throw new ArgumentNullException ("permissions");

			UserId = user.UserId;
			Permissions = permissions;
		}

		public SetPermissionsMessage (int userId, IEnumerable<Permission> permissions)
			: this()
		{
			if (permissions == null)
				throw new ArgumentNullException ("permissions");

			UserId = userId;
			Permissions = permissions;
		}

		public int UserId
		{
			get; set;
		}

		public IEnumerable<Permission> Permissions
		{
			get; set;
		}

		public override void WritePayload (IValueWriter writer)
		{
			writer.WriteInt32 (UserId);
			
			var perms = Permissions.ToList();
			writer.WriteInt32 (perms.Count);
			for (int i = 0; i < perms.Count; ++i)
				perms[i].Serialize (writer);
		}

		public override void ReadPayload (IValueReader reader)
		{
			UserId = reader.ReadInt32();

			int permissionCount = reader.ReadInt32();
			Permission[] permissions = new Permission[permissionCount];
			for (int i = 0; i < permissions.Length; ++i)
				permissions[i] = new Permission (reader);
			
			Permissions = permissions;
		}
	}
}