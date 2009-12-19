using System;
using System.Collections.Generic;
using Gablarski.Messages;

namespace Gablarski.Server
{
	public interface IConnectionManager
	{
		void Connect (IConnection connection);
		bool GetIsConnected (IConnection connection);
		void Disconnect (IConnection connection);
		
		void Send (MessageBase message, Func<IConnection, bool> predicate);
	}
}