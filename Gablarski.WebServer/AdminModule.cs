// Copyright (c) 2009, Eric Maupin
// All rights reserved.

// Redistribution and use in source and binary forms, with
// or without modification, are permitted provided that
// the following conditions are met:

// - Redistributions of source code must retain the above 
//   copyright notice, this list of conditions and the
//   following disclaimer.

// - Redistributions in binary form must reproduce the above
//   copyright notice, this list of conditions and the
//   following disclaimer in the documentation and/or other
//   materials provided with the distribution.

// - Neither the name of Gablarski nor the names of its
//   contributors may be used to endorse or promote products
//   derived from this software without specific prior
//   written permission.

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
using HttpServer;
using HttpServer.HttpModules;
using HttpServer.Sessions;

namespace Gablarski.WebServer
{
	public class AdminModule
		: HttpModule
	{
		public AdminModule (ConnectionManager cmanager)
		{
			this.cmanager = cmanager;
		}

		public override bool Process (IHttpRequest request, IHttpResponse response, IHttpSession session)
		{
			if (request.UriParts.Length > 0 && request.UriParts[0] == "admin")
			{
				cmanager.ProcessSession (session, response);
				try
				{
					if (!((bool)session["loggedIn"]))
						response.Redirect ("/admin/login.html");
					else
					{
						if (request.UriParts.Length > 1)
						{
							switch (request.UriParts[1])
							{
								
							}
						}
						else
						{
							
						}
					}
				}
				catch (Exception ex)
				{
					var writer = new StreamWriter (response.Body);
					writer.Write ("<pre>");
					writer.Write (ex);
					writer.Write ("</pre>");
					writer.Flush();
					response.Send();
				}

				return true;
			}

			return false;
		}

		private readonly ConnectionManager cmanager;

		private void ShowLogin()
		{
			
		}
	}
}