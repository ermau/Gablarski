using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gablarski.Server
{
	/// <summary>
	/// Interface for integrated backend providers.
	/// </summary>
	public interface IBackendProvider
		: IAuthenticationProvider, IChannelProvider, IPermissionsProvider
	{
		/// <summary>
		/// Gets user permissions for a specific channel.
		/// </summary>
		/// <param name="channelId">The channel to check the user's permissions on.</param>
		/// <param name="userId">The player to check the permissions on.</param>
		/// <returns>The permissions for the player in the specific channel.</returns>
		IEnumerable<Permission> GetPermissions (int channelId, int userId);
	}
}