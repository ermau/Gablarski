// Copyright (c) 2011-2013, Eric Maupin
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
using Tempest;

namespace Gablarski.Messages
{
	public class PermissionsMessage
		: GablarskiMessage
	{
		public PermissionsMessage()
			: base (GablarskiMessageType.Permissions)
		{
		}

		public PermissionsMessage (int ownerId, IEnumerable<Permission> permissions)
			: base (GablarskiMessageType.Permissions)
		{
			this.OwnerId = ownerId;
			this.Permissions = permissions;
		}

		public override bool Encrypted
		{
			get { return true; }
		}

		public int OwnerId
		{
			get;
			set;
		}

		public IEnumerable<Permission> Permissions
		{
			get;
			set;
		}

		public override void WritePayload (ISerializationContext context, IValueWriter writer)
		{
			writer.WriteInt32 (this.OwnerId);

			if (this.Permissions != null)
			{
				writer.WriteInt32 (this.Permissions.Count());
				foreach (var p in this.Permissions)
					p.Serialize (context, writer);
			}
			else
				writer.WriteInt32 (0);
		}

		public override void ReadPayload (ISerializationContext context, IValueReader reader)
		{
			this.OwnerId = reader.ReadInt32();

			int npermissions = reader.ReadInt32();
			Permission[] permissions = new Permission[npermissions];
			for (int i = 0; i < permissions.Length; ++i)
				permissions[i] = new Permission (context, reader);
			
			this.Permissions = permissions;
		}
	}
}