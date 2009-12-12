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
using System.IO;
using Gablarski.Messages;
using HttpServer;
using HttpServer.HttpModules;
using HttpServer.Sessions;
using Newtonsoft.Json;

namespace Gablarski.WebServer
{
	public class QueryModule
		: HttpModule
	{
		public QueryModule(ConnectionManager manager)
		{
			this.cmanager = manager;
			this.serializer = new JsonSerializer();
			this.serializer.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
		}

		#region Overrides of HttpModule

		public override bool Process (IHttpRequest request, IHttpResponse response, IHttpSession session)
		{
			if (request.UriParts.Length > 0 && request.UriParts[0] == "query")
			{
				cmanager.ProcessSession (session, response);

				var bodyWriter = new StreamWriter (response.Body);
				
				try
				{
					var result = cmanager.SendAndReceive<QueryServerMessage, QueryServerResultMessage> (new QueryServerMessage(), session);
				
					JsonTextWriter writer = new JsonTextWriter (bodyWriter);
					serializer.Serialize (writer, result);

					#if !DEBUG
					response.ContentType = "application/json";
					#endif

					response.AddHeader ("X-JSON", "true");
				}
				catch (Exception ex)
				{
					bodyWriter.Write ("<pre>");
					bodyWriter.Write (ex);
					bodyWriter.Write ("</pre>");
				}

				bodyWriter.Flush();
				response.Send();
				
				return true;
			}

			return false;
		}

		#endregion

		private readonly ConnectionManager cmanager;
		private readonly JsonSerializer serializer;
	}
}