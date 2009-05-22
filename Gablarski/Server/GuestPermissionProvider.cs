using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gablarski.Server
{
	public class GuestPermissionProvider
		: IPermissionsProvider
	{
		public IEnumerable<Permission> GetPermissions (string username)
		{
			if (this.admins.Contains (username))
			{
				return GetNamesAsPermissions (
					PermissionName.Login,
					PermissionName.KickPlayer);
			}

			return GetNamesAsPermissions (PermissionName.Login);
		}

		public void SetAdmin (string username)
		{
			if (!this.admins.Contains (username))
				this.admins.Add (username);
		}

		private readonly HashSet<string> admins = new HashSet<string> ();

		private IEnumerable<Permission> GetNamesAsPermissions(params PermissionName[] names)
		{
			for (int i = 0; i < names.Length; ++i)
				yield return new Permission (names[i], true);
		}
	}
}