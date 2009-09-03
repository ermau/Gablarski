using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gablarski.Server
{
	public interface IPermissionsProvider
	{
		event EventHandler<PermissionsChangedEventArgs> PermissionsChanged;

		IEnumerable<Permission> GetPermissions (int userId);
	}

	public class PermissionsChangedEventArgs
		: EventArgs
	{
		public PermissionsChangedEventArgs (int userId)
		{
			this.UserId = userId;
		}

		public int UserId
		{
			get; private set;
		}
	}
}