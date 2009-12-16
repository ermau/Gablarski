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
using System.Configuration;
using System.Linq;
using System.Net;
using System.Text;
using Gablarski.Messages;
using Gablarski.Server;
using Gablarski.WebServer.Config;
using HttpServer;
using HttpServer.Helpers;
using HttpServer.HttpModules;
using HttpServer.Rendering;
using HttpServer.Rendering.Tiny;
using HttpServer.Rules;
using HttpServer.Sessions;

namespace Gablarski.WebServer
{
	public class WebServerConnectionProvider
		: IConnectionProvider
	{
		public int Port
		{
			get { return this.port; }
			set { this.port = value; }
		}

		#region Implementation of IConnectionProvider

		public event EventHandler<ConnectionlessMessageReceivedEventArgs> ConnectionlessMessageReceived;
		public event EventHandler<ConnectionEventArgs> ConnectionMade;

		public void SendConnectionlessMessage(MessageBase message, EndPoint endpoint)
		{
			throw new NotSupportedException();
		}

		public void StartListening (IServerContext context)
		{
			WebServerConfiguration config = (WebServerConfiguration) ConfigurationManager.GetSection ("webserver");

			var sstore = new MemorySessionStore();
			server = new HttpServer.HttpServer (sstore);

			TemplateManager templates = new TemplateManager (new ResourceTemplateLoader());
			templates.AddType (typeof (WebHelper));
			templates.Add ("tiny", new TinyGenerator());

			ConnectionManager cmanager = new ConnectionManager();
			cmanager.ConnectionProvider = this;
			cmanager.Server = server;

			ControllerModule controller = new ControllerModule();
			controller.Add (new LoginController (templates, cmanager, context));
			//controller.Add (new UserController (cmanager));
			//controller.Add (new ChannelController (cmanager));

			//server.Add (new QueryModule(cmanager));
			//server.Add (new FileResourceModule (config.Theme.Path));
			//server.Add (new LoginModule(cmanager));
			//server.Add (new AdminModule(cmanager));
			server.Add (controller);
			
			server.Start (IPAddress.Any, this.Port);
		}

		public void StopListening()
		{
			server.Stop();
			server = null;
		}

		#endregion

		private HttpServer.HttpServer server;
		private int port = 6113;

		internal protected void OnConnectionMade (ConnectionEventArgs e)
		{
			var made = this.ConnectionMade;
			if (made != null)
				made (this, e);
		}
	}
}