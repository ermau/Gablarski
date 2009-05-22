using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gablarski
{
	public class Permissions
	{
		public Permissions ()
		{
			this.permissions = new HashSet<Permission> ();
		}

		public Permissions (IEnumerable<Permission> permissions)
		{
			this.permissions = new HashSet<Permission> (permissions);
		}

		public bool CanLogin
		{
			get { return GetPermission (PermissionName.Login); }
		}

		private readonly HashSet<Permission> permissions;

		private bool GetPermission (PermissionName name)
		{
			var perm = this.permissions.Where (p => p.Name == name).FirstOrDefault ();
			return (perm != null && perm.IsAllowed);
		}
	}
}