﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gablarski.Server;
using Gablarski.Network;
using System.Diagnostics;
using Gablarski.Client;
using System.Threading;
using Gablarski.Messages;
using Gablarski;
using Gablarski.Media.Sources;

namespace GablarskiTester
{
	class Program
	{
		static GablarskiServer server;
		static GablarskiClient client;
		static string username = "Rogue Jedi";

		static void Main (string[] args)
		{
			server = new GablarskiServer (new GuestUserProvider ());
			server.AddConnectionProvider (new ServerNetworkConnectionProvider { Port = 6112 });

			client = new GablarskiClient (new ClientNetworkConnection ());
			client.ReceivedToken += client_ReceivedTokenResult;
			client.ReceivedLogin += client_ReceivedLogin;
			client.ReceivedSource += client_ReceivedSource;
			client.Connect ("localhost", 6112);

			while (true)
			{
				Thread.Sleep (1);
			}
		}

		static void client_ReceivedSource (object sender, ReceivedSourceEventArgs e)
		{
			Trace.WriteLine ("Source received: " + e.Result + " " + e.Source.GetType ());
		}

		static void client_ReceivedLogin (object sender, ReceivedLoginEventArgs e)
		{
			Trace.WriteLine ("Login result: " + e.Result.Succeeded + " " + e.Result.FailureReason);

			client.RequestSource (typeof(VoiceSource), 1);
		}

		static void client_ReceivedTokenResult (object sender, ReceivedTokenEventArgs e)
		{
			Trace.WriteLine ("Token result: " + e.Result);

			client.Login (username);
		}
	}
}