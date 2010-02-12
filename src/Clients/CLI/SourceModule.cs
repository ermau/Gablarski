using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Gablarski.Audio;
using Gablarski.Client;
using Cadenza.Collections;

namespace Gablarski.Clients.CLI
{
	public class SourceModule
		: GClientModule
	{
		public SourceModule (GablarskiClient client, TextWriter writer)
			: base (client, writer)
		{
			client.Sources.AudioSourceStarted += OnSourceStarted;
			client.Sources.AudioSourceStopped += OnSourceStopped;
		}

		public override bool Process (string line)
		{
			var parts = CommandLine.Parse (line);
			if (parts.Count == 0 || parts[0].ToLower() == "sources")
				return false;

			if (parts.Count == 1 || parts[0].ToLower() == "sources")
			{
				Console.WriteLine ("Source commands:");
				Console.WriteLine ("source request <name>");
				Console.WriteLine ("source request <name> <bitrate>");
				Console.WriteLine ("source request <name> <bitrate> <frameSize>");
				return true;
			}

			switch (parts[1])
			{
				case "request":
				{
					if (parts.Count == 2)
						Client.Sources.Request (parts[2], 1, 512);
					else if (parts.Count == 3)
					{
						short frameSize;
						int bitrate;
						if (!Int16.TryParse (parts[3], out frameSize) || !Int32.TryParse (parts[4], out bitrate))
							Console.WriteLine ("source request <name> <bitrate> <frameSize>");
						else if (frameSize < 128 || frameSize > 1024)
							Console.WriteLine ("source request <name> <bitrate> <frameSize>");
						else if (bitrate < 32000 || bitrate > 128000)
							Console.WriteLine ("source request <name> <bitrate> <frameSize>");
						else
							Client.Sources.Request (parts[2], 1, frameSize, bitrate);
					}

					return true;
				}

				case "list":
				{
					Console.WriteLine ("My sources:");
					foreach (var source in Client.Sources.Mine)
					{
						Console.WriteLine ("\"{0}\"", source.Name);
						Console.WriteLine ("ID:	        {0}", source.Id);
						Console.WriteLine ("Muted:      {0}", source.IsMuted);
						Console.WriteLine ("Frequency:  {0}", source.Frequency);
						Console.WriteLine ("Channels:   {0}", source.Channels);
						Console.WriteLine ("Frame size: {0}", source.FrameSize);
						Console.WriteLine ("Bitrate:    {0}", source.Bitrate);
					}

					foreach (var source in Client.Sources)
					Console.WriteLine("Owner: {0}", Client.Users[source.OwnerId].Nickname);

					return true;
				}
			}

			return false;
		}

		private void OnSourceStopped(object sender, AudioSourceEventArgs e)
		{
			var user = Client.Users[e.Source.OwnerId];
			if (user == null)
				return;

			Writer.WriteLine("{0} stopped talking.", user.Nickname);
		}

		private void OnSourceStarted(object sender, AudioSourceEventArgs e)
		{
			var user = Client.Users[e.Source.OwnerId];
			if (user == null)
				return;

			Writer.WriteLine("{0} started talking.", user);
		}
	}
}