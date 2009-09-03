using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gablarski.Messages
{
	public class PermissionsMessage
		: ServerMessage
	{
		public PermissionsMessage()
			: base (ServerMessageType.Permissions)
		{
		}

		public PermissionsMessage (int ownerId, IEnumerable<Permission> permissions)
			: base (ServerMessageType.Permissions)
		{
			this.OwnerId = ownerId;
			this.Permissions = permissions;
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

		public override void WritePayload (IValueWriter writer)
		{
			writer.WriteInt32 (this.OwnerId);

			if (this.Permissions != null)
			{
				writer.WriteInt32 (this.Permissions.Count());
				foreach (var p in this.Permissions)
					p.Serialize (writer);
			}
			else
				writer.WriteInt32 (0);
		}

		public override void ReadPayload (IValueReader reader)
		{
			this.OwnerId = reader.ReadInt32();

			int npermissions = reader.ReadInt32();
			Permission[] permissions = new Permission[npermissions];
			for (int i = 0; i < permissions.Length; ++i)
				permissions[i] = new Permission (reader);
			
			this.Permissions = permissions;
		}
	}
}