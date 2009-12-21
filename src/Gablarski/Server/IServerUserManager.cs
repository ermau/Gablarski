using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gablarski.Messages;

namespace Gablarski.Server
{
	public interface IServerUserManager
		: IConnectionManager, IIndexedEnumerable<int, UserInfo>
	{
		void Login (IConnection connection, UserInfo user);
		bool GetIsLoggedIn (UserInfo user);
		bool GetIsLoggedIn (IConnection connection);

		void Join (IConnection connection, UserInfo user);
		bool GetIsJoined (UserInfo user);
		bool GetIsJoined (IConnection connection);

		IConnection GetConnection (UserInfo user);
		UserInfo GetUser (IConnection connection);

		void Send (MessageBase message, Func<IConnection, UserInfo, bool> predicate);
	}
}