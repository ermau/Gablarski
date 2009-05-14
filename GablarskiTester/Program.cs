using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gablarski.OpenAL;
using Gablarski.OpenAL.Providers;
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
		static CaptureDevice microphone;
		static PlaybackProvider speakers;
		static SourcePool<IMediaSource> sources;
		static Context c;

		static void Main (string[] args)
		{
			Console.WriteLine ("Playback devices:");
			foreach (var device in OpenAL.PlaybackDevices)
				Console.WriteLine (device.Name);

			Console.WriteLine ("Capture devices:");
			foreach (var device in OpenAL.CaptureDevices)
				Console.WriteLine (device.Name);

			microphone = OpenAL.DefaultCaptureDevice.Open (44100, AudioFormat.Mono16Bit);
			microphone.SamplesAvailable += microphone_SamplesAvailable;

			speakers = new PlaybackProvider();
			speakers.Device = OpenAL.DefaultPlaybackDevice.Open();

			server = new GablarskiServer (new ServerInfo
			{
				ServerName = "Rogue Jedi's Server",
				ServerDescription = "Development Server",
			}, new GuestUserProvider ());

			server.AddConnectionProvider (new ServerNetworkConnectionProvider { Port = 6112 });

			client = new GablarskiClient (new ClientNetworkConnection ());
			client.Login (username);
			client.ReceivedLoginResult += client_ReceivedLogin;
			client.ReceivedSource += client_ReceivedSource;
			client.ReceivedAudioData += client_ReceivedAudioData;
			client.Connect ("localhost", 6112);

			bool recording = false;
			ConsoleKeyInfo key;
			while (true)
			{
				key = Console.ReadKey(true);
				if (key.Key != ConsoleKey.V)
				{
					Thread.Sleep (1);
					continue;
				}

				if (!recording)
				{
					Console.WriteLine ("Starting capture.");
					microphone.StartCapture();
					recording = true;
				}
				else
				{
					Console.WriteLine ("Ending capture.");
					microphone.StopCapture();
					recording = false;
				}
			}
		}

		static void client_ReceivedAudioData (object sender, ReceivedAudioEventArgs e)
		{
			speakers.QueuePlayback (e.Source, e.AudioData);
		}

		static void microphone_SamplesAvailable (object sender, SamplesAvailableEventArgs e)
		{
			client.SendAudioData (client.VoiceSource, microphone.GetSamples());
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
	}
}