using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HttpServer;
using HttpServer.HttpModules;
using HttpServer.Sessions;

namespace Gablarski.WebServer
{
	public class AdminModule
		: HttpModule
	{
		#region Overrides of HttpModule

		public override bool Process (IHttpRequest request, IHttpResponse response, IHttpSession session)
		{
			if (request.UriParts.Length > 0 && request.UriParts[0] == "admin")
			{
				ConnectionManager.ProcessSession (session, response);

				return true;
			}

			return false;
		}

		#endregion
	}
}