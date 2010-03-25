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
using System.Linq;
using System.Text;
using System.Threading;
using Gablarski.Messages;
using HttpServer;
using HttpServer.Sessions;
using Cadenza;

namespace Gablarski.WebServer
{
	public class ConnectionManager
	{
		public ConnectionManager (IHttpSessionStore httpSessionStore)
		{
			this.sessionStore = httpSessionStore;
		}

		public WebServerConnectionProvider ConnectionProvider
		{
			get; set;
		}

		public HttpServer.HttpServer Server
		{
			get; set;
		}

		public TimeSpan SessionTtl
		{
		    get { return sessionTtl; }
		    set { sessionTtl = value; }
		}

		public TimeSpan TimeBetweenScans
		{
		    get { return timeBetweenScans; }
		    set { timeBetweenScans = value; }
		}

		public bool ProcessSession (IHttpSession session, IHttpResponse response)
		{
			bool newSession = false;
			lock (connections)
			{
				sessionStore.Cleanup();
				if (DateTime.Now.Subtract (lastScanned) > TimeBetweenScans)
				    KillOldSessions();

				//if (!sessions.ContainsKey (session.Id))
				//    newSession = true;

				newSession = !connections.ContainsKey (session);

				//sessions[session.Id] = session;

				if (newSession)
				{
					WebServerConnection connection = new WebServerConnection (session);

					session["mqueue"] = new List<MessageBase>();
					session["connection"] = connection;
					session["loggedIn"] = false;
					sessionStore.Save (session);

					//response.Cookies.Add (new ResponseCookie (Server.SessionCookieName, session.Id, DateTime.Now.Add (SessionTtl)));

					ConnectionProvider.OnConnectionMade (new ConnectionEventArgs (connection));
					connections.Add (session, connection);
				}

				return (bool)session["loggedIn"];
			}
		}

		public void SaveSession (IHttpSession session)
		{
			lock (connections)
			{
				sessionStore.Save (session);
			}
		}

		public TReceive Receive<TReceive> (IHttpSession session)
			where TReceive : MessageBase
		{
			List<MessageBase> mqueue = (List<MessageBase>)session["mqueue"];

			TReceive receive = null;
			while (receive == null)
			{
				Thread.Sleep (1);

				lock (mqueue)
					receive = mqueue.OfType<TReceive>().FirstOrDefault();
			}
			
			lock (mqueue)
				mqueue.Remove (receive);

			return receive;
		}

		public TReceive SendAndReceive<TReceive> (MessageBase message, IHttpSession session)
			where TReceive : MessageBase
		{
			lock (connections)
			{
				connections[session].Receive (message);
			}

			return Receive<TReceive> (session);
		}

		public TReceive SendAndReceive<TReceive, TError> (MessageBase message, IHttpSession session, out TError error)
			where TReceive : MessageBase
			where TError : MessageBase
		{

			List<MessageBase> mqueue = (List<MessageBase>)session["mqueue"];
			lock (connections)
			{
				connections[session].Receive (message);
			}

			int i = 0;

			error = null;
			TReceive receive = null;
			do
			{
				Thread.Sleep (1);

				lock (mqueue)
				{
					receive = mqueue.OfType<TReceive>().FirstOrDefault();
					if (receive == null)
						error = mqueue.OfType<TError>().FirstOrDefault();
				}
			} while (++i < 30000 && receive == null && error == null);
			
			lock (mqueue)
			{
				if (receive != null)
					mqueue.Remove (receive);

				if (error != null)
					mqueue.Remove (error);
			}

			return receive;
		}

		private TimeSpan sessionTtl = TimeSpan.FromMinutes (15);
		private TimeSpan timeBetweenScans = TimeSpan.FromMinutes (1);
		private DateTime lastScanned = DateTime.Now;

		private readonly Dictionary<IHttpSession, WebServerConnection> connections = new Dictionary<IHttpSession, WebServerConnection>();
		private readonly IHttpSessionStore sessionStore;

		private void KillOldSessions ()
		{
		    lock (connections)
		    {
		        foreach (var kvp in connections.ToList())
		        {
		            if (DateTime.Now.Subtract (kvp.Key.Accessed) <= sessionTtl)
		                continue;

		        	kvp.Value.Disconnect();
		            connections.Remove (kvp.Key);
		        }
		    }

		    lastScanned = DateTime.Now;
		}
	}
}