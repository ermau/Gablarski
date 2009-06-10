using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gablarski.Server
{
	public class GuestPermissionProvider
		: IPermissionsProvider
	{
		public IEnumerable<Permission> GetPermissions (long playerId)
		{
			if (this.admins.Contains (playerId))
				return GetNamesAsPermissions (Permission.GetAllNames());

			return GetNamesAsPermissions (PermissionName.Login, PermissionName.RequestSource, PermissionName.ChangeChannel);
		}

		public void SetAdmin (long playerId)
		{
			if (playerId == 0)
				throw new ArgumentException ("Guests can not be admins.");

			if (!this.admins.Contains (playerId))
				this.admins.Add (playerId);
		}

		private readonly HashSet<long> admins = new HashSet<long> ();

		private IEnumerable<Permission> GetNamesAsPermissions (IEnumerable<PermissionName> names)
		{
			return names.Select (n => new Permission (n, true));
		}

		private IEnumerable<Permission> GetNamesAsPermissions (params PermissionName[] names)
		{
			for (int i = 0; i < names.Length; ++i)
				yield return new Permission (names[i], true);
		}
	}
}