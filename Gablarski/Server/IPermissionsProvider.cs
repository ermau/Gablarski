using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gablarski.Server
{
	/// <summary>
	/// Contract for providers of permissions
	/// </summary>
	public interface IPermissionsProvider
	{
		/// <summary>
		/// Fired when persmissions have changed
		/// </summary>
		event EventHandler<PermissionsChangedEventArgs> PermissionsChanged;

		/// <summary>
		/// Gets whether or not the permissions provided can be updated.
		/// </summary>
		bool UpdatedSupported { get; }

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