// Copyright (c) 2010, Eric Maupin
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
using Gablarski.Messages;
using HttpServer;
using HttpServer.HttpModules;
using HttpServer.Sessions;
using Newtonsoft.Json;

namespace Gablarski.WebServer
{
	public class LoginModule
		: SectionModule
	{
		public LoginModule (ConnectionManager cmanager)
			: base (cmanager, "login")
		{
		}

		protected override bool MustBeLoggedIn
		{
			get { return false; }
		}

		protected override bool ProcessSection (IHttpRequest request, IHttpResponse response, IHttpSession session)
		{
			IHttpInput input = (request.Method.ToLower() == "post") ? request.Form : request.QueryString;
			/*if (request.Method.ToLower() != "post")
				return false;*/

			if (!input.Contains ("username") || !input.Contains ("password"))
			{
				WriteAndFlush (response, "{ \"error\": \"Invalid request\" }");
				return true;
			}

			var result = Connections.SendAndReceive<LoginResultMessage> (
							new LoginMessage { Username = input["username"].Value, Password = input["password"].Value }, session);
			
			if (!result.Result.Succeeded)
				WriteAndFlush (response, JsonConvert.SerializeObject (new { result.Result, SessionId = session.Id }));
			else
			{
				var pmsg = Connections.Receive<PermissionsMessage> (session);

				if (pmsg.Permissions.CheckPermission (PermissionName.AdminPanel))
				{
					session["loggedIn"] = true;
					Connections.SaveSession (session);

					WriteAndFlush (response, JsonConvert.SerializeObject (new { result.Result, SessionId = session.Id, pmsg.Permissions }));
				}
				else
				{
					WriteAndFlush (response, "{ \"error\": \"Insufficient permissions\" }");
				}
			}

			return true;
		}
	}
}