// Copyright (c) 2009, Eric Maupin
// All rights reserved.
//
// Redistribution and use in source and binary forms, with
// or without modification, are permitted provided that
// the following conditions are met:
//
// - Redistributions of source code must retain the above 
//   copyright notice, this list of conditions and the
//   following disclaimer.
//
// - Redistributions in binary form must reproduce the above
//   copyright notice, this list of conditions and the
//   following disclaimer in the documentation and/or other
//   materials provided with the distribution.
//
// - Neither the name of Gablarski nor the names of its
//   contributors may be used to endorse or promote products
//   or services derived from this software without specific
//   prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS
// AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED
// WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
// WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR
// PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT
// HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT,
// INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
// (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE
// GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
// INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY,
// WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
// NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
// THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH
// DAMAGE.

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
		public LoginModule (ConnectionManager cmanager)
		{
			this.cmanager = cmanager;
		}

		public override bool Process (IHttpRequest request, IHttpResponse response, IHttpSession session)
		{
			if (request.UriParts.Length > 0 && request.UriParts[0].ToLower() == "login" && request.Method.ToLower() == "post")
			{
				if (!request.Form.Contains ("username") || !request.Form.Contains ("password"))
				{
					WriteAndFlush (response, "Invalid Login");
					return true;
				}

				cmanager.ProcessSession (session, response);

				var result = cmanager.SendAndReceive<LoginMessage, LoginResultMessage> (
								new LoginMessage { Username = request.Form["username"].Value, Password = request.Form["password"].Value }, session);

				var permissions = cmanager.Receive<PermissionsMessage> (session);

				if (!result.Result.Succeeded)
					WriteAndFlush (response, "Invalid Login");
				else if (permissions.Permissions.CheckPermission (PermissionName.AdminPanel))
				{
					session["loggedIn"] = true;
					cmanager.SaveSession (session);
					response.Redirect ("/admin");
				}
				else
				{
					WriteAndFlush (response, "Insufficient permissions");
				}

				return true;
			}

			return false;
		}

		private readonly ConnectionManager cmanager;

		private static void WriteAndFlush (IHttpResponse response, string body)
		{
			var writer = new StreamWriter (response.Body);
			writer.WriteLine (body);
			writer.Flush();
			response.Send();
		}
	}
}