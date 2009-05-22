using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gablarski
{
	public enum PermissionName
	{
		Login = 1,
		KickPlayer = 2
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
}