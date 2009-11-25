using System;
using System.Collections.Generic;
using Gablarski.Messages;

namespace Gablarski.Server
{
	public interface IServerUserManager
		: IUserManager
	{
		void Login (UserInfo user);
		bool GetIsLoggedIn (UserInfo user);
		
		void Connect (IConnection connection);
		void Associate (IConnection connection, UserInfo user);
		
		void Send (MessageBase message, UserInfo user);
		void Send (MessageBase message, IEnumerable<UserInfo> users);
		void Send (MessageBase message, Func<IConnection, bool> predicate);
		void Send (MessageBase message, Func<IConnection, UserInfo, bool> predicate);
	}
}