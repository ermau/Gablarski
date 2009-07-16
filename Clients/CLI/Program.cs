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
		public static GablarskiClient Client;
		public static GablarskiServer Server;

		private static string username;
		private static string password;
		private static string nickname;
		private static bool voiceSource = false;

		private static bool startClient = true;

		private readonly static List<AudioSource> Sources = new List<AudioSource>();

		private static IPlaybackProvider playbackProvider;
		private static ICaptureProvider captureProvider;
		private static ClientAudioSource captureSource;
		private static VoiceActivation voiceActivation;

		public static IPlaybackProvider PlaybackProvider
		{
			get
			{
				if (playbackProvider == null)
					playbackProvider = new PlaybackProvider();

				return playbackProvider;
			}
		}

		public static ICaptureProvider CaptureProvider
		{
			get
			{
				if (captureProvider == null)
					captureProvider = new CaptureProvider();

				return captureProvider;
			}
		}

		[STAThread]
		public static void Main (string[] args)
		{
			bool trace = false;
			List<string> tracers = new List<string>();

			string clientConnection = typeof (NetworkClientConnection).AssemblyQualifiedName;
			string host = String.Empty;
			int port = 6112;
			bool defaultAudio = false;

			bool startServer = false;
			string serverLogo = String.Empty;
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

				{ "s|server",		"Starts a local Server.",										s => startServer = true },
				{ "serverlogo=",	"Specifies a server logo URL.",									v => serverLogo = v },
				{ "conprovider=",	"Adds a connection provider to the Server.",					connectionProviders.Add	},
			};

			foreach (string unused in options.Parse (args))
				Console.WriteLine ("Unrecognized option: " + unused);

			if (!startServer && (String.IsNullOrEmpty (nickname) || String.IsNullOrEmpty (host)))
			{
				options.WriteOptionDescriptions (Console.Out);
				Environment.Exit (1);
			}
			else if (startServer && (String.IsNullOrEmpty (nickname) || String.IsNullOrEmpty (host)))
			{
				startClient = false;
			}

			if (startClient && !username.IsEmpty() && password.IsEmpty())
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
				Server = new GablarskiServer (new ServerSettings { ServerLogo = serverLogo }, new GuestUserProvider(), new GuestPermissionProvider(), new LobbyChannelProvider());
				
				if (connectionProviders.Count == 0)
					Server.AddConnectionProvider (new NetworkServerConnectionProvider { Port = port });
				else
					FindTypes<IConnectionProvider> (connectionProviders, Server.AddConnectionProvider);

				Server.Start();
			}

			if (defaultAudio)
			{
				SelectPlayback (PlaybackProvider.DefaultDevice, Console.Out);
				SelectCapture (CaptureProvider.DefaultDevice, Console.Out);
			}
			
			if (startClient)
			{
				Client = new GablarskiClient (new NetworkClientConnection());
				Client.Connected += ClientConnected;
				Client.ConnectionRejected += ClientConnectionRejected;
				Client.Disconnected += ClientDisconnected;

				Client.Sources.ReceivedSource += SourcesReceivedSource;
				Client.Sources.ReceivedAudio += SourcesReceivedAudio;

				Client.Connect (host, port);
			}

			string fullcommand;
			while ((fullcommand = Console.ReadLine()) != null)
			{
				if (fullcommand.IsEmpty())
					continue;

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
						if (startClient)
							Client.Disconnect();

						if (startServer)
							Server.Shutdown();

						Environment.Exit (0);
						break;

					case "source":
						if (!startClient)
							break;

						int channels = 1;
						int bitrate = 0;
						string request = null;
						var sourceOptions = new OptionSet
						{
							{ "l|list",			"Lists current Sources.",						v => ListSources (Console.Out)},

							{ "r=|request=",	"Requests a source.",							v => request = v },
							{ "b=|bitrate=",	"Bitrate of the source to request. (>32000)",	(int b) => bitrate = b },
							{ "c=|channels=",	"Channels of the source to request. (1-2)",		(int c) => channels = c }
						};
						sourceOptions.Parse (cmdopts);

						if (request != null)
						{
							if (channels < 1 || channels > 2)
								sourceOptions.WriteOptionDescriptions (Console.Out);
							else if (bitrate < 32000 && bitrate != 0)
								sourceOptions.WriteOptionDescriptions (Console.Out);

							Client.Sources.Request (request, channels, bitrate);
						}

						break;

					case "select":
						if (!startClient)
							break;

						string input = null;
						string output = null;

						var soptions = new OptionSet
						{
							{ "s|selected", "Displays currently selected devices.", s => { Console.WriteLine ("Output: " + ((PlaybackProvider.Device != null) ? PlaybackProvider.Device. ToString() : "None"));
							                                                             	Console.WriteLine ("Input: " + ((CaptureProvider.Device !=null) ? CaptureProvider.Device. ToString() : "None")); } },
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
								SelectPlayback (PlaybackProvider.DefaultDevice, Console.Out);
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
								SelectCapture (CaptureProvider.DefaultDevice, Console.Out);
						}

						break;

					case "capture":
						if (!startClient)
							break;

						bool stop = false;
						bool useVoiceActivation = false;
						//int sourceId = 0;

						var coptions = new OptionSet
						{
							{ "s|stop", v => stop = (v != null)},
							//{ "source:", (int v) => sourceId = v },
							{ "v|vactivation", "Uses voice activation.", v => useVoiceActivation = (v != null) }
						};
						coptions.Parse (cmdopts);

						if (stop)
						{
							if (voiceActivation != null)
							{
								voiceActivation.StopListening();
								voiceActivation.Talking -= VaTalking;
								voiceActivation = null;
							}
							else if (CaptureProvider.IsCapturing)
								CaptureProvider.EndCapture();
						}
						else
						{
							ClientAudioSource source;

							lock (Sources)
							{
								if (Sources.Count == 0)
								{
									Console.WriteLine ("No Sources to capture to.");
									break;
								}

								//source = (sourceId != 0) ? Sources.OfType<ClientAudioSource>().FirstOrDefault (s => s.Id == sourceId) : Sources.OfType<ClientAudioSource>().First();
								source = captureSource;
							}

							if (source == null)
								Console.WriteLine ("Source not found.");
							else
							{
								if (useVoiceActivation)
								{
									voiceActivation = new VoiceActivation (CaptureProvider, captureSource);
									voiceActivation.Talking += VaTalking;
									voiceActivation.Listen ((Int16.MaxValue) / 2);
								}
								else
								{
									CaptureProvider.SamplesAvailable += OnSamplesAvailable;
									if (!CaptureProvider.IsCapturing)
										CaptureProvider.StartCapture();
								}

								//if (captureSource == null)
								//    CaptureProvider.SamplesAvailable += OnSamplesAvailable;

								//captureSource = source;
								//if (!CaptureProvider.IsCapturing)
								//    CaptureProvider.StartCapture();
							}
						}

						break;

					default:
						WriteCommands (Console.Out);
						break;
				}
			}
		}

		static void VaTalking (object sender, TalkingEventArgs e)
		{
			captureSource.SendAudioData (e.Samples, Client.CurrentUser.CurrentChannelId);
		}

		static void SourcesReceivedAudio (object sender, ReceivedAudioEventArgs e)
		{
			PlaybackProvider.QueuePlayback (e.Source, e.AudioData);
		}

		static void OnSamplesAvailable (object sender, SamplesAvailableEventArgs e)
		{
			if (captureSource != null)
				captureSource.SendAudioData (CaptureProvider.ReadSamples (captureSource.FrameSize), Client.Users.Current.CurrentChannelId);
		}

		static void ListSources (TextWriter writer)
		{
			lock (Sources)
			{
				foreach (var source in Sources)
					writer.WriteLine (source.GetType().Name + ": " + source.Id + " Bitrate: " + source.Bitrate);
			}
		}
		
		static void SourcesReceivedSource (object sender, ReceivedSourceEventArgs e)
		{
			if (!e.Source.OwnerId.Equals (Client.CurrentUser.UserId))
				return;

			Console.WriteLine ("Received own source. Name: " + e.Source.Name + " Id: " + e.Source.Id + " Owner: " + e.Source.OwnerId + " Bitrate: " + e.Source.Bitrate);

			captureSource = (ClientAudioSource)e.Source;

			lock (Sources)
			{
				Sources.Add (e.Source);
			}
		}

		static void SelectCapture (IDevice device, TextWriter writer)
		{
			CaptureProvider.Device = device;
			writer.WriteLine (CaptureProvider.Device + " selected.");
		}

		static void SelectPlayback (IDevice device, TextWriter writer)
		{
			PlaybackProvider.Device = device;
			writer.WriteLine (device + " selected.");
		}

		static void WriteCommands (TextWriter writer)
		{
			writer.WriteLine ("Commands:");
			if (startClient)
			{
				writer.WriteLine ("requestsource: Requests an AudioSource.");
				writer.WriteLine ("select: Selects an input and/or an output device.");
				writer.WriteLine ("capture: Starts or stops capturing audio.");
			}
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
				Client.Sources.Request ("voice", 1);
		}

		static void ClientConnectionRejected (object sender, RejectedConnectionEventArgs e)
		{
			Console.WriteLine ("Connection rejected: " + e.Reason);
		}

		static void ClientConnected (object sender, EventArgs e)
		{
			Console.WriteLine ("Connected.");
			Client.CurrentUser.ReceivedLoginResult += CurrentUserReceivedLoginResult;
			Client.CurrentUser.Login (nickname, username, password);
		}
	}
}