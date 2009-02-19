using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gablarski.Server
{
	public class Permission
	{
		public Permission (string name)
		{
			this.Name = name;
		}

		public Permission (string name, bool isAllowed)
			: this (name)
		{
			this.IsAllowed = isAllowed;
		}

		public virtual string Name
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