using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gablarski.Server
{
	public interface IServerPersistance
	{
		/// <summary>
		/// Persists information about the user.
		/// </summary>
		/// <param name="user">The user to persist.</param>
		void Persist (UserInfo user);

		/// <summary>
		/// Loads persisted data about the user.
		/// </summary>
		/// <param name="userId">The id of the user to load.</param>
		/// <returns><c>null</c> if the user does not exist.</returns>
		UserInfo GetUser (int userId);
	}
}