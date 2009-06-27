using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Gablarski.Client;
using Gablarski.Media.Sources;
using Gablarski.Network;
using Gablarski.OpenAL.Providers;
using Gablarski.Server;
using Mono.Options;
using System.Linq;

namespace Gablarski.Clients.CLI
{
	public class Program
	{
		public static GablarskiClient client;
		public static GablarskiServer server;

		private static string username;
		private static string password;
		private static string nickname;
		private static bool voiceSource = false;

		private readonly static List<MediaSourceBase> sources = new List<MediaSourceBase>();

		private readonly static IPlaybackProvider playbackProvider = new PlaybackProvider();
		private	readonly static ICaptureProvider captureProvider = new CaptureProvider();
		private static ClientMediaSource captureSource;

		[STAThread]
		public static void Main (string[] args)
		{
			string host = String.Empty;
			int port = 6112;
			bool trace = false;
			bool startServer = false;
			bool defaultAudio = false;
			

			List<string> tracers = new List<string>();
			string clientConnection = typeof (ClientNetworkConnection).AssemblyQualifiedName;
			List<string> connectionProviders = new List<string>();

			OptionSet options = new OptionSet
			{
				{ "h=|host=",		h => host = h },
				{ "p:|port:",		(int p) => port = p },
				{ "u=|username=",	u => username = u },
				{ "pw=|password=",	p => password = p },
				{ "n=|nickname=",	n => nickname = n },
				
				{ "voicesource",	"Requests a voice source upon login.",							v => voiceSource = (v != null) },
				{ "defaultaudio",	"Selects the default input and output audio.",					v => defaultAudio = (v != null) },

				{ "v|verbose",		"Turns on tracing, detailed debug invormation",					v => trace = (v != null) },
				{ "tracer=",		"Adds a tracer to the list (supply assembly qualified name.)",	tracers.Add },

				{ "s|server",		"Starts a local server.",										s => startServer = true },
				{ "conprovider=",	"Adds a connection provider to the server.",					connectionProviders.Add	},
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
			{
				if (tracers.Count == 0)
					Trace.Listeners.Add (new DetailedConsoleTraceListener { TraceOutputOptions = TraceOptions.Timestamp });
				else
					FindTypes<TraceListener> (tracers, t => Trace.Listeners.Add (t));
			}

			if (startServer)
			{
				server = new GablarskiServer (new ServerSettings(), new GuestUserProvider(), new GuestPermissionProvider(), new LobbyChannelProvider());
				
				if (connectionProviders.Count == 0)
					server.AddConnectionProvider (new ServerNetworkConnectionProvider());
				else
					FindTypes<IConnectionProvider> (connectionProviders, server.AddConnectionProvider);

				server.Start();
			}

			if (defaultAudio)
			{
				SelectPlayback (playbackProvider.DefaultDevice, Console.Out);
				SelectCapture (captureProvider.DefaultDevice, Console.Out);
			}
			
			client = new GablarskiClient (new ClientNetworkConnection ());
			client.Connected += ClientConnected;
			client.ConnectionRejected += ClientConnectionRejected;
			client.Disconnected += ClientDisconnected;
			client.Sources.ReceivedSource += SourcesReceivedSource;

			client.Connect (host, port);

			string fullcommand;
			while ((fullcommand = Console.ReadLine()) != null)
			{
				int spacei = fullcommand.IndexOf (' ');
				spacei = (spacei != -1) ? spacei : fullcommand.Length - 1;
				string cmd = fullcommand.Substring (0, spacei + 1).Trim();
				string cmdopts = fullcommand.Substring (spacei, fullcommand.Length - spacei).Trim();

				switch (cmd)
				{
					case "exit":
					case "quit":
					case "close":
					case "disconnect":
						client.Disconnect();
						Environment.Exit (0);
						break;

					case "source":
						int channels = 1;
						int bitrate = 0;
						bool request = false;
						var sourceOptions = new OptionSet
						{
							{ "l|list",			"Lists current sources.",						v => ListSources (Console.Out)},

							{ "r|request",		"Requests a source.",							v => request = (v != null) },
							{ "b=|bitrate=",	"Bitrate of the source to request. (>32000)",	(int b) => bitrate = b },
							{ "c=|channels=",	"Channels of the source to request. (1-2)",		(int c) => channels = c }
						};
						sourceOptions.Parse (cmdopts);

						if (request)
						{
							if (channels < 1 || channels > 2)
								sourceOptions.WriteOptionDescriptions (Console.Out);
							else if (bitrate < 32000 && bitrate != 0)
								sourceOptions.WriteOptionDescriptions (Console.Out);

							client.Sources.Request (channels, bitrate);
						}

						break;

					case "select":
						string input = null;
						string output = null;

						var soptions = new OptionSet
						{
							{ "s|selected", "Displays currently selected devices.", s => { Console.WriteLine ("Output: " + ((playbackProvider.Device != null) ? playbackProvider.Device. ToString() : "None"));
							                                                             	Console.WriteLine ("Input: " + ((captureProvider.Device !=null) ? captureProvider.Device. ToString() : "None")); } },
							{ "d|devices",	"Displays the output and input devices to choose from.",	s => DisplayDevices() },
							{ "i:|input:",	"Selects an input device. (Default if not specified.)",		i => input = i ?? String.Empty },
							{ "o:|output:",	"Selects an output device. (Default if not specified.)",	o => output = o ?? String.Empty }
						};
						
						if (soptions.Parse (cmdopts).Count > 0)
							soptions.WriteOptionDescriptions (Console.Out);

						if (output != null)
						{
							output = output.Trim();
							if (output != String.Empty)
							{
								var device = OpenAL.OpenAL.PlaybackDevices.FirstOrDefault (d => d.Name == output);
								if (device == null)
									Console.WriteLine (output + " not found.");
								else
									SelectPlayback (device, Console.Out);
							}
							else
								SelectPlayback (playbackProvider.DefaultDevice, Console.Out);
						}

						if (input != null)
						{
							input = input.Trim();
							if (input != String.Empty)
							{
								var device = OpenAL.OpenAL.CaptureDevices.FirstOrDefault (d => d.Name == input);
								if (device == null)
									Console.WriteLine (input + " not found.");
								else
									SelectCapture (device, Console.Out);
							}
							else
								SelectCapture (captureProvider.DefaultDevice, Console.Out);
						}

						break;

					case "capture":
						bool stop = false;
						bool voiceActivation = false;
						int sourceId = 0;

						var coptions = new OptionSet
						{
							{ "s|stop", v => stop = (v != null)},
							{ "source:", (int v) => sourceId = v },
							//{ "v|vactivation", "Uses voice activation.", v => voiceActivation = (v != null) }
						};
						coptions.Parse (cmdopts);

						if (stop)
						{
							if (captureProvider.IsCapturing)
								captureProvider.EndCapture();
						}
						else
						{
							ClientMediaSource source;

							lock (sources)
							{
								if (sources.Count == 0)
								{
									Console.WriteLine ("No sources to capture to.");
									break;
								}

								source = (sourceId != 0) ? sources.OfType<ClientMediaSource>().FirstOrDefault (s => s.Id == sourceId) : sources.OfType<ClientMediaSource>().First();
							}

							if (source == null)
								Console.WriteLine ("Source not found.");
							else
							{
								if (captureSource == null)
									captureProvider.SamplesAvailable += OnSamplesAvailable;

								captureSource = source;
								if (!captureProvider.IsCapturing)
									captureProvider.StartCapture();
							}
						}

						break;

					default:
						WriteCommands (Console.Out);
						break;
				}
			}
		}

		static void OnSamplesAvailable (object sender, SamplesAvailableEventArgs e)
		{
			if (captureSource != null)
				captureSource.SendAudioData (captureProvider.ReadSamples(e.Samples), client.Users.Current.CurrentChannelId);
		}

		static void ListSources (TextWriter writer)
		{
			lock (sources)
			{
				foreach (var source in sources)
					writer.WriteLine (source.GetType().Name + ": " + source.Id + " Bitrate: " + source.Bitrate);
			}
		}
		
		static void SourcesReceivedSource (object sender, ReceivedSourceEventArgs e)
		{
			if (!e.Source.OwnerId.Equals (client.CurrentUser.UserId))
				return;

			Console.WriteLine ("Received own source. Id: " + e.Source.Id + " Owner: " + e.Source.OwnerId + " Bitrate: " + e.Source.Bitrate);
			
			lock (sources)
			{
				sources.Add (e.Source);
			}
		}

		static void SelectCapture (IDevice device, TextWriter writer)
		{
			captureProvider.Device = device;
			writer.WriteLine (captureProvider.Device + " selected.");
		}

		static void SelectPlayback (IDevice device, TextWriter writer)
		{
			playbackProvider.Device = device;
			writer.WriteLine (device + " selected.");
		}

		static void WriteCommands (TextWriter writer)
		{
			writer.WriteLine ("Commands:");
			writer.WriteLine ("requestsource: Requests an AudioSource.");
			writer.WriteLine ("select: Selects an input and/or an output device.");
		}

		static void DisplayDevices()
		{
			Console.WriteLine ("Playback devices:");
			foreach (var device in OpenAL.OpenAL.PlaybackDevices)
			{
				if (device == OpenAL.OpenAL.DefaultPlaybackDevice)
					Console.Write ("[Default] ");

				Console.WriteLine ("\"" + device.Name + "\" ");
			}

			Console.WriteLine();
			Console.WriteLine ("Capture devices:");
			foreach (var device in OpenAL.OpenAL.CaptureDevices)
			{
				if (device == OpenAL.OpenAL.DefaultCaptureDevice)
					Console.Write ("[Default] ");

				Console.WriteLine ("\"" + device.Name + "\"");
			}
		}

		static void FindTypes<T> (IEnumerable<string> typeNames, Action<T> typeFound)
			where T : class
		{
			foreach (string type in typeNames)
			{
				try
				{
					Type tprovider = Type.GetType (type, false, true);
					if (tprovider == null)
						Console.Error.WriteLine (type + " was not found.");
					else if (!typeof (T).IsAssignableFrom (tprovider))
						Console.Error.WriteLine (type + " is not an IConnectionProvider");
					else
						typeFound ((T)Activator.CreateInstance (tprovider));
				}
				catch (Exception ex)
				{
					Console.Error.WriteLine (type + ex.GetType().Name + ": " + ex.Message);
				}
			}
		}

		static void ClientDisconnected (object sender, EventArgs e)
		{
			Console.WriteLine ("Disconnected.");
			Environment.Exit (0);
		}

		static void CurrentUserReceivedLoginResult (object sender, ReceivedLoginResultEventArgs e)
		{
			if (e.Result.Succeeded)
				Console.WriteLine ("Logged in.");
			else
				Console.WriteLine ("Login failed: " + e.Result.ResultState);

			if (voiceSource)
				client.Sources.Request (1);
		}

		static void ClientConnectionRejected (object sender, RejectedConnectionEventArgs e)
		{
			Console.WriteLine ("Connection rejected: " + e.Reason);
		}

		static void ClientConnected (object sender, EventArgs e)
		{
			Console.WriteLine ("Connected.");
			client.CurrentUser.ReceivedLoginResult += CurrentUserReceivedLoginResult;
			client.CurrentUser.Login (nickname, username, password);
		}
	}
}