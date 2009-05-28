using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gablarski.Server
{
	public interface IBackendProvider
		: IUserProvider, IChannelProvider, IPermissionsProvider
	{
		IEnumerable<Permission> GetPermissions (long channelId, long playerId);
	}
}