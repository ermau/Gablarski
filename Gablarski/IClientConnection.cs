using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gablarski
{
	public interface IClientConnection
		: IConnection
	{
		event EventHandler Connected;

		void Connect (string host, int port);
	}
}