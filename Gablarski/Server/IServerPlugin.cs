using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gablarski.Server
{
	public interface IServerPlugin
		: IPlugin
	{
		void Attach (GablarskiServer server);
		void Detatch (GablarskiServer server);
	}
}