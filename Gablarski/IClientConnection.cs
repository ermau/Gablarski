using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gablarski
{
	public interface IClientConnection
		: IConnection
	{
		void Connect (string host, int port);
	}
}