using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Gablarski.Messages;
using HttpServer;
using HttpServer.HttpModules;
using HttpServer.Sessions;

namespace Gablarski.WebServer
{
	public class LoginModule
		: HttpModule
	{
		public override bool Process(IHttpRequest request, IHttpResponse response, IHttpSession session)
		{
			if (request.UriParts.Length > 0 && request.UriParts[0].ToLower() == "login" && request.Method.ToLower() == "post")
			{
				if (!request.Form.Contains ("username") || !request.Form.Contains ("password"))
				{
					InvalidLogin (response);
					return true;
				}

				ConnectionManager.ProcessSession (session, response);

				ConnectionManager.SendAndReceive<LoginMessage, LoginResultMessage> (
					new LoginMessage { Username = request.Form["username"].Value, Password = request.Form["password"].Value }, session);

				return true;
			}

			return false;
		}

		private void InvalidLogin (IHttpResponse response)
		{
			var writer = new StreamWriter (response.Body);
			writer.WriteLine("Invalid login");
			writer.Flush();
			response.Send();
		}
	}
}