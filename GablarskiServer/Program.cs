﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Reflection;
using Gablarski.Server.Providers;
using Lidgren.Network;
using Gablarski.Server;
using Gablarski.Client;

namespace Gablarski.Server
{
	class Program
	{
		public static NetServer Server;
		public static IAuthProvider Authentication;

		static void Main (string[] args)
		{
			Trace.UseGlobalLock = true;
			Trace.Listeners.Add(new ConsoleListener());

			if (!Debugger.IsAttached)
			{
				AppDomain.CurrentDomain.UnhandledException += delegate (object sender, UnhandledExceptionEventArgs e)
				{
					var ex = (Exception)e.ExceptionObject;
					Trace.WriteLine (ex.Message);
					Trace.WriteLine (ex.StackTrace);
				};
			}

			Authentication = new NicknameAuthenticationProvider();
			
			Console.WriteLine ("Gablarski Server v" + Assembly.GetExecutingAssembly ().GetName ().Version + " starting up...");
			
			GablarskiServer server = new GablarskiServer(Authentication);
			server.ClientConnected += (sender, e) => Trace.WriteLine ("Client connected.");
			server.Start();
		}
	}

	public class ConsoleListener
		: TraceListener
	{
		public override void Write (string message)
		{
			Console.Write (message);
		}

		public override void WriteLine(string message)
		{
			Console.WriteLine (message);
		}
	}
}