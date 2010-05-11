// Copyright (c) 2010, Eric Maupin
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
using System.IO;
using System.Linq;
using Cadenza;
using Gablarski.Audio;
using Gablarski.Client;

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

		private int lastTalkedId;
		public override bool Process (string line)
		{
			var parts = CommandLine.Parse (line);
			if (parts.Count == 0 || parts[0].ToLower() != "sources")
				return false;

			if (parts.Count == 1 && parts[0].ToLower() == "sources")
			{
				Writer.WriteLine ("Sources commands:");
				Writer.WriteLine ("sources list");
				Writer.WriteLine ("sources request <name>");
				Writer.WriteLine ("sources request <name> <bitrate>");
				Writer.WriteLine ("sources request <name> <bitrate> <frameSize>");
				Writer.WriteLine ("sources talk <id|name>");
				Writer.WriteLine ("sources talk <id|name> <channel>");
				Writer.WriteLine ("sources endtalk <id|name>");
				return true;
			}

			switch (parts[1])
			{
				case "request":
				{
					if (parts.Count == 3)
						Client.Sources.Request (parts[2], AudioFormat.Mono16bitLPCM, 512);
					else if (parts.Count == 4)
					{
						short frameSize;
						int bitrate;
						if (!Int16.TryParse (parts[3], out frameSize) || !Int32.TryParse (parts[4], out bitrate))
							Writer.WriteLine ("source request <name> <bitrate> <frameSize>");
						else if (frameSize < 128 || frameSize > 1024)
							Writer.WriteLine ("source request <name> <bitrate> <frameSize>");
						else if (bitrate < 32000 || bitrate > 128000)
							Writer.WriteLine ("source request <name> <bitrate> <frameSize>");
						else
							Client.Sources.Request (parts[2], AudioFormat.Mono16bitLPCM, frameSize, bitrate);
					}

					return true;
				}

				case "list":
				{
					if (Client.Sources.Mine.Any())
					{
						Writer.WriteLine();
						Writer.WriteLine ("My sources:");

						foreach (var source in Client.Sources.Mine)
						{
							Writer.WriteLine ("\"{0}\"", source.Name);
							Writer.WriteLine ("ID:	  		    	{0}", source.Id);
							Writer.WriteLine ("Channels:			{0}", source.Channels);
							Writer.WriteLine ("Bits per sample:		{0}", source.BitsPerSample);
							Writer.WriteLine ("Muted:				{0}", source.IsMuted);
							Writer.WriteLine ("Frequency:			{0}", source.SampleRate);
							Writer.WriteLine ("Frame size:			{0}", source.FrameSize);
							Writer.WriteLine ("Bitrate:				{0}", source.Bitrate);
							Writer.WriteLine();
						}
					}

					foreach (var source in Client.Sources)
					{
						Writer.WriteLine();
						Writer.WriteLine ("{1}: \"{0}\"", source.Name, Client.Users[source.OwnerId].Nickname);
						Writer.WriteLine ("ID:	  		    	{0}", source.Id);
						Writer.WriteLine ("Channels:			{0}", source.Channels);
						Writer.WriteLine ("Bits per sample:		{0}", source.BitsPerSample);
						Writer.WriteLine ("Muted:				{0}", source.IsMuted);
						Writer.WriteLine ("Frequency:			{0}", source.SampleRate);
						Writer.WriteLine ("Frame size:			{0}", source.FrameSize);
						Writer.WriteLine ("Bitrate:				{0}", source.Bitrate);
					}

					return true;
				}

				case "talk":
				{
					AudioSource source = FindSource ((parts.Count > 2) ? parts[2] : null);

					if (source == null)
						Writer.WriteLine ("Own source '{0}' not found.", parts[2].Trim().ToLower());
					else
					{
						lastTalkedId = source.Id;

						ChannelInfo channel = null;
						if (parts.Count == 4)
						{
							int channelId;
							if (Int32.TryParse (parts[3], out channelId))
								channel = Client.Channels[channelId];
						}

						if (channel == null)
							channel = Client.CurrentChannel;

						Client.Audio.BeginCapture (source, channel);
					}

					return true;
				}

				case "endtalk":
				{
					AudioSource source = FindSource ((parts.Count > 2) ? parts[2] : null);
					
					if (source == null)
						Writer.WriteLine ("Own source '{0}' not found.", parts[2].Trim().ToLower());
					else
						Client.Audio.EndCapture (source);

					break;
				}
			}

			return false;
		}

		private AudioSource FindSource (string part)
		{
			AudioSource source;

			int sourceId;
			if (part.IsNullOrWhitespace() && this.lastTalkedId != 0)
				source = Client.Sources[this.lastTalkedId];
			else if (Int32.TryParse (part, out sourceId))
				source = Client.Sources.Mine.SingleOrDefault (a => a.Id == sourceId);
			else
				source = Client.Sources.Mine.SingleOrDefault (a => a.Name.Trim().ToLower() == part.Trim().ToLower());
			return source;
		}

		private void OnSourceStopped (object sender, AudioSourceEventArgs e)
		{
			var user = Client.Users[e.Source.OwnerId];
			if (user == null)
				return;

			Writer.WriteLine ("{0} stopped talking.", user.Nickname);
		}

		private void OnSourceStarted (object sender, AudioSourceEventArgs e)
		{
			var user = Client.Users[e.Source.OwnerId];
			if (user == null)
				return;

			Writer.WriteLine ("{0} started talking.", user.Nickname);
		}
	}
}