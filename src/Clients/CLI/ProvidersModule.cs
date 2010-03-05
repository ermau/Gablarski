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
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Linq;
using Gablarski.Audio;
using Gablarski.Client;
using Cadenza;

namespace Gablarski.Clients.CLI
{
	public class ProvidersModule
		: GClientModule
	{
		public ProvidersModule (GablarskiClient client, TextWriter writer)
			: base (client, writer)
		{
			var container = new CompositionContainer (new DirectoryCatalog (".", "Gablarski*.dll"));
			container.ComposeParts (this);

			client.Sources.ReceivedSourceList += OnReceivedSourceList;
			client.Sources.ReceivedAudioSource += OnReceivedAudioSource;
		}

		[ImportMany]
		public IEnumerable<IPlaybackProvider> Playback
		{
			get; set;
		}

		[ImportMany]
		public IEnumerable<ICaptureProvider> Capture
		{
			get; set;
		}

		private IPlaybackProvider selectedPlayback;
		public IPlaybackProvider SelectedPlayback
		{
			get { return this.selectedPlayback; }
			set
			{
				if (this.selectedPlayback == value)
					return;

				if (this.selectedPlayback != null)
				{
					Client.Audio.Detach (this.selectedPlayback);
					this.selectedPlayback.Dispose();
				}

				this.selectedPlayback = value;
				this.selectedPlayback.Device = value.DefaultDevice;

				var otherSources = Client.Sources.Where (s => s.OwnerId != Client.CurrentUser.UserId).ToList();
				if (otherSources.Any())
					Client.Audio.Attach (value, otherSources, new AudioEnginePlaybackOptions());
				
				Writer.WriteLine ("{0} selected.", value.GetType().FullName);
			}
		}

		private ICaptureProvider selectedCapture;
		public ICaptureProvider SelectedCapture
		{
			get { return this.selectedCapture; }
			set
			{
				if (this.selectedCapture == value)
					return;

				if (this.selectedCapture != null)
				{
					Client.Audio.Detach (this.selectedCapture);
					this.selectedCapture.Dispose();
				}

				this.selectedCapture = value;
				this.selectedCapture.Device = value.DefaultDevice;

				if (Client.Sources.Mine.Any())
				{
					Client.Audio.Attach (value, Client.Sources.Mine.Single(),
					                     new AudioEngineCaptureOptions {Mode = AudioEngineCaptureMode.Explicit});
				}
				
				Writer.WriteLine ("{0} selected.", value.GetType().FullName);
			}
		}

		public override bool Process (string line)
		{
			var parts = CommandLine.Parse (line);
			if (parts.Count == 0 || parts[0].ToLower() != "providers")
				return false;

			if (parts.Count == 1 && parts[0].ToLower() == "providers")
			{
				Writer.WriteLine ("Providers commands:");
				Writer.WriteLine ("providers list <playback|capture>");
				Writer.WriteLine ("providers select <type> <provider>");
				return true;
			}

			switch (parts[1])
			{
				case "list":
				{
					if (parts.Count < 3 || parts[2].ToLower().Trim() == "playback")
					{
						if (SelectedPlayback != null)
							Writer.WriteLine ("Current playback provider: {0}{1}", Playback.GetType().FullName, Environment.NewLine);

						Writer.WriteLine ("Playback Providers:");
						foreach (IPlaybackProvider p in Playback)
							Writer.WriteLine (p.GetType().FullName);

						Writer.WriteLine();
					}

					if (parts.Count < 3 || parts[2].ToLower().Trim() == "capture")
					{
						if (SelectedCapture != null)
							Writer.WriteLine ("Current capture provider: {0}{1}", Capture.GetType().FullName, Environment.NewLine);

						Writer.WriteLine ("Capture Providers:");
						foreach (ICaptureProvider p in Capture)
							Writer.WriteLine (p.GetType().FullName);
					}

					break;
				}

				case "select":
				{
					if (parts.Count == 4)
					{
						int providerIndex;
						string provider = parts[3].Trim().Remove (" ").ToLower();
						string type = parts[2].Trim().Remove (" ").ToLower();
						switch (type)
						{
							case "iplaybackprovider":
							case "playback":
							case "playbackprovider":
								
								if (Int32.TryParse (provider, out providerIndex))
								{
									if (providerIndex < Playback.Count())
										break;

									SelectedPlayback = Playback.ElementAt (providerIndex);
								}
								else
								{
									IPlaybackProvider playback = Playback.FirstOrDefault (p => p.GetType().Name.ToLower() == provider || p.GetType().FullName.ToLower() == provider);
									if (playback == null)
										break;

									SelectedPlayback = playback;
								}

								return true;

							case "icaptureprovider":
							case "capture":
							case "captureprovider":
								if (Int32.TryParse (provider, out providerIndex))
								{
									if (providerIndex < Playback.Count())
										break;

									SelectedPlayback = Playback.ElementAt (providerIndex);
								}
								else
								{
									ICaptureProvider capture = Capture.FirstOrDefault (p => p.GetType().Name.ToLower() == provider || p.GetType().FullName.ToLower() == provider);
									if (capture == null)
										break;

									SelectedCapture = capture;
								}

								return true;
						}
					}

					Writer.WriteLine ("providers select <type> <provider>");
					break;
				}
			}

			return true;
		}

		private void OnReceivedAudioSource (object sender, ReceivedAudioSourceEventArgs e)
		{
			if (SelectedPlayback != null && e.Result == Messages.SourceResult.NewSource)
				Client.Audio.Attach (SelectedPlayback, e.Source, new AudioEnginePlaybackOptions());
			else if (SelectedCapture != null && e.Result == Messages.SourceResult.Succeeded)
			{
				Client.Audio.Attach (SelectedCapture, e.Source,
				                     new AudioEngineCaptureOptions { Mode = AudioEngineCaptureMode.Explicit });
			}
		}

		private void OnReceivedSourceList (object sender, ReceivedListEventArgs<AudioSource> e)
		{
			if (SelectedPlayback != null)
			{
				var otherSources = Client.Sources.Where (s => s.OwnerId != Client.CurrentUser.UserId).ToList();
				if (otherSources.Any())
					Client.Audio.Attach (SelectedPlayback, otherSources, new AudioEnginePlaybackOptions());
			}

			if (SelectedCapture != null && Client.Sources.Mine.Any())
			{
				Client.Audio.Attach (SelectedCapture, Client.Sources.Mine.Single(),
				                     new AudioEngineCaptureOptions {Mode = AudioEngineCaptureMode.Explicit});
			}
		}
	}
}