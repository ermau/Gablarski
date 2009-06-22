using System;
using System.Diagnostics;
using Gablarski.Client;
using Gablarski.Media.Sources;
using Gablarski.Network;
using Gablarski.Server;
using Mono.Options;

namespace Gablarski.Clients.CLI
{
	public class Program
	{
		public static GablarskiClient client;
		public static GablarskiServer server;

		private static string username;
		private static string password;
		private static string nickname;

		private static bool voiceSource;
		private static bool musicSource;

		private static AudioSource voice;
		private static AudioSource music;

		[STAThread]
		public static void Main (string[] args)
		{
			string host = String.Empty;
			int port = 6112;
			bool trace = false;
			bool startServer = false;

			OptionSet options = new OptionSet
			{
				{ "h=|host=",		h => host = h },
				{ "p:|port:",		(int p) => port = p },
				{ "v|verbose",		"Turns on tracing, detailed debug invormation",	v => trace = true },
				{ "u=|username=",	u => username = u },
				{ "pw=|password=",	p => password = p },
				{ "n=|nickname=",	n => nickname = n },
				{ "s|server",		"Starts a local server.",						s => startServer = true },
				{ "voicesource",	"Requests a voice source upon login.",			s => voiceSource = true },
				{ "musicsource",	"Requests a music source upon login.",			s => musicSource = true },
			};

			foreach (string unused in options.Parse (args))
				Console.WriteLine ("Unrecognized option: " + unused);

			if (String.IsNullOrEmpty (nickname) || String.IsNullOrEmpty (host))
			{
				options.WriteOptionDescriptions (Console.Out);
				Environment.Exit (1);
			}

			if (!username.IsEmpty() && password.IsEmpty())
			{
				Console.WriteLine ("Can not login without a password.");
				Environment.Exit (1);
			}

			if (trace)
				Trace.Listeners.Add (new ConsoleTraceListener ());

			if (startServer)
			{
				server = new GablarskiServer (new ServerSettings(), new GuestUserProvider(), new GuestPermissionProvider(), new LobbyChannelProvider());
				server.AddConnectionProvider (new ServerNetworkConnectionProvider (port));
				server.Start();
			}
			
			client = new GablarskiClient (new ClientNetworkConnection ());
			client.Connected += client_Connected;
			client.ConnectionRejected += client_ConnectionRejected;
			client.Disconnected += client_Disconnected;
			client.Sources.ReceivedSource += Sources_ReceivedSource;

			client.Connect (host, port);
		}

		static void client_Disconnected (object sender, EventArgs e)
		{
			Console.WriteLine ("Disconnected.");
			Environment.Exit (0);
		}

		static void Sources_ReceivedSource (object sender, ReceivedSourceEventArgs e)
		{
			if (e.Source.OwnerId != client.CurrentUser.UserId)
				return;
			
			if (e.Source.Bitrate > 64000)
				music = (AudioSource)e.Source;
			else
				voice = (AudioSource)e.Source;
		}

		static void CurrentUser_ReceivedLoginResult (object sender, ReceivedLoginResultEventArgs e)
		{
			if (e.Result.Succeeded)
				Console.WriteLine ("Logged in.");
			else
				Console.WriteLine ("Login failed: " + e.Result.ResultState);

			if (voiceSource)
				client.Sources.Request (1);

			if (musicSource)
				client.Sources.Request (1, 96000);
		}

		static void client_ConnectionRejected (object sender, RejectedConnectionEventArgs e)
		{
			Console.WriteLine ("Connection rejected: " + e.Reason);
		}

		static void client_Connected (object sender, EventArgs e)
		{
			Console.WriteLine ("Connected");
			client.CurrentUser.ReceivedLoginResult += CurrentUser_ReceivedLoginResult;
			client.CurrentUser.Login (nickname, username, password);
		}
	}
}