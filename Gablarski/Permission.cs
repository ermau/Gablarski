using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gablarski
{
	public enum PermissionName
	{
		Login = 1,
		KickPlayer = 2,
		RequestSource = 3
	}

	public class Permission
	{
		public Permission (PermissionName name)
		{
			this.Name = name;
		}

		public Permission (PermissionName name, bool isAllowed)
			: this (name)
		{
			this.IsAllowed = isAllowed;
		}

		public virtual PermissionName Name
		{
			get;
			private set;
		}

		public virtual bool IsAllowed
		{
			get;
			set;
		}
	}

	public static class PermissionExtensions
	{
		public static bool CanLogin (this IEnumerable<Permission> self)
		{
			return self.GetPermission (PermissionName.Login);
		}

		public static bool CanRequestSource (this IEnumerable<Permission> self)
		{
			return self.GetPermission (PermissionName.RequestSource);
		}

		public static bool CanKickPlayer (this IEnumerable<Permission> self)
		{
			return self.GetPermission (PermissionName.KickPlayer);
		}

		public static bool GetPermission (this IEnumerable<Permission> self, PermissionName name)
		{
			var perm = self.Where (p => p.Name == name).FirstOrDefault ();
			return (perm != null && perm.IsAllowed);
		}
	}
}