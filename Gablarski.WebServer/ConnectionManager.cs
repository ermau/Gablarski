using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Gablarski.Messages;
using HttpServer;
using HttpServer.Sessions;
using Mono.Rocks;

namespace Gablarski.WebServer
{
	public static class ConnectionManager
	{
		static ConnectionManager()
		{
			SessionStore = new MemorySessionStore();
		}

		public static WebServerConnectionProvider ConnectionProvider
		{
			get; set;
		}

		public static HttpServer.HttpServer Server
		{
			get; set;
		}

		public static TimeSpan SessionTtl
		{
			get { return sessionTtl; }
			set { sessionTtl = value; }
		}

		public static TimeSpan TimeBetweenScans
		{
			get { return timeBetweenScans; }
			set { timeBetweenScans = value; }
		}

		public static void ProcessSession (IHttpSession session, IHttpResponse response)
		{
			bool newSession = false;
			lock (sessions)
			{
				if (!sessions.ContainsKey (session.Id))
					newSession = true;
				
				sessions[session.Id] = session;

				if (DateTime.Now.Subtract (lastScanned) > TimeBetweenScans)
					KillOldSessions();

				if (newSession)
				{
					WebServerConnection connection = new WebServerConnection (session);

					session["mqueue"] = new List<MessageBase>();
					session["connection"] = connection;
					session["loggedIn"] = false;
					SessionStore.Save (session);

					response.Cookies.Add (new ResponseCookie (Server.SessionCookieName, session.Id, DateTime.Now.Add (SessionTtl)));

					ConnectionProvider.OnConnectionMade (new ConnectionEventArgs (connection));
					connections.Add (session.Id, connection);
				}
			}
		}

		public static void SaveSession (IHttpSession session)
		{
			lock (sessions)
			{
				SessionStore.Save (session);
			}
		}

		public static TReceive Receive<TReceive> (IHttpSession session)
			where TReceive : MessageBase
		{
			List<MessageBase> mqueue = (List<MessageBase>)session["mqueue"];

			TReceive receive = null;
			while (receive == null)
			{
				Thread.Sleep (1);

				receive = mqueue.OfType<TReceive>().FirstOrDefault();
			}
			
			mqueue.Remove (receive);

			return receive;
		}

		public static TReceive SendAndReceive<TSend, TReceive> (TSend message, IHttpSession session)
			where TSend : MessageBase
			where TReceive : MessageBase
		{
			lock (sessions)
			{
				connections[session.Id].Receive (message);
			}

			return Receive<TReceive> (session);
		}

		public static TReceive SendAndReceive<TSend, TReceive, TError> (TSend message, IHttpSession session, out TError error)
			where TSend : MessageBase
			where TReceive : MessageBase
			where TError : MessageBase
		{

			List<MessageBase> mqueue = (List<MessageBase>)session["mqueue"];
			lock (sessions)
			{
				connections[session.Id].Receive (message);
			}

			error = null;
			TReceive receive = null;
			while (receive == null || error == null)
			{
				Thread.Sleep (1);

				receive = mqueue.OfType<TReceive>().FirstOrDefault();
				if (receive == null)
					error = mqueue.OfType<TError>().FirstOrDefault();
			}
			
			mqueue.Remove (receive);

			return receive;
		}

		private static TimeSpan sessionTtl = TimeSpan.FromMinutes (15);
		private static TimeSpan timeBetweenScans = TimeSpan.FromMinutes (1);
		private static DateTime lastScanned = DateTime.Now;

		private static readonly Dictionary<string, IHttpSession> sessions = new Dictionary<string, IHttpSession>();
		private static readonly Dictionary<string, WebServerConnection> connections = new Dictionary<string, WebServerConnection>();
		private static readonly MemorySessionStore store = new MemorySessionStore();

		private static void KillOldSessions ()
		{
			lock (sessions)
			{
				foreach (var session in sessions.Values.ToList())
				{
					if (DateTime.Now.Subtract (session.Accessed) <= TimeBetweenScans)
						continue;

					sessions.Remove (session.Id);
					connections[session.Id].Disconnect();
					connections.Remove (session.Id);
				}
			}

			lastScanned = DateTime.Now;
		}

		private static IHttpSessionStore SessionStore
		{
			get; set;
		}
	}
}