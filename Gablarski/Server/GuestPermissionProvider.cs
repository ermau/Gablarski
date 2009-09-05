using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gablarski.Server
{
	public class GuestPermissionProvider
		: IPermissionsProvider
	{
		public event EventHandler<PermissionsChangedEventArgs> PermissionsChanged;

		public bool UpdatedSupported
		{
			get { return false; }
		}

		public IEnumerable<Permission> GetPermissions (int userID)
		{
			if (this.admins.Contains (userID))
				return GetNamesAsPermissions (Permission.GetAllNames());

			if (userID != 0) // User
			{
				return GetNamesAsPermissions (PermissionName.Login, PermissionName.ChangeChannel, PermissionName.AddChannel,
				                              PermissionName.EditChannel, PermissionName.DeleteChannel,
				                              PermissionName.RequestChannelList, PermissionName.RequestSource);
			}
			else // Non-user client.
				return GetNamesAsPermissions (PermissionName.Login, PermissionName.RequestChannelList);
		}


		public void SetAdmin (int userId)
		{
			if (userId == 0)
				throw new ArgumentException ("Guests can not be admins.");

			if (!this.admins.Contains (userId))
				this.admins.Add (userId);

			OnPermissionsChanged (new PermissionsChangedEventArgs (userId));
		}

		private readonly HashSet<int> admins = new HashSet<int> ();

		private IEnumerable<Permission> GetNamesAsPermissions (IEnumerable<PermissionName> names)
		{
			return names.Select (n => new Permission (n, true));
		}

		private IEnumerable<Permission> GetNamesAsPermissions (params PermissionName[] names)
		{
			for (int i = 0; i < names.Length; ++i)
				yield return new Permission (names[i], true);
		}

		protected void OnPermissionsChanged (PermissionsChangedEventArgs e)
		{
			var changed = this.PermissionsChanged;
			if (changed != null)
				changed (this, e);
		}
	}
}