// Copyright (c) 2011, Eric Maupin
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
using System.Linq;
using System.Speech.Recognition;
using Gablarski.Client;
using Gablarski.Clients;
using Gablarski.Clients.Input;

namespace Gablarski.SpeechNotifier
{
	[Export (typeof (ISpeechRecognizer))]
	public class SpeechRecognizer
		: ISpeechRecognizer
	{
		public event EventHandler<CommandStateChangedEventArgs> CommandStateChanged;

		public string Name
		{
			get { return "Windows Speech Recognition"; }
		}

		public ITextToSpeech TTS
		{
			get;
			set;
		}

		public void Open()
		{
			this.recognition = new SpeechRecognitionEngine();
			this.recognition.SpeechRecognized += OnSpeechRecognized;
			this.recognition.MaxAlternates = 1;
			this.recognition.SetInputToDefaultAudioDevice();

			GrammarBuilder builder = new GrammarBuilder();
			builder.Append (new SemanticResultKey ("command", new Choices ("mute", "unmute")));
			builder.Append (new Choices (" ", "me"));

			this.muteGrammar = new Grammar (builder);
			this.recognition.LoadGrammar (this.muteGrammar);
		}

		public void Close()
		{
			if (this.recognition != null)
				this.recognition.Dispose();

			this.recognition = null;
			this.muteGrammar = null;
		}

		public void StartRecognizing()
		{
			lock (this.recognition)
				this.recognition.RecognizeAsync (RecognizeMode.Single);
		}

		public void StopRecognizing()
		{
			lock (this.recognition)
				this.recognition.RecognizeAsyncCancel();
		}

		public void Update (IEnumerable<IChannelInfo> channels, IEnumerable<IUserInfo> users)
		{
			if (channels == null)
				throw new ArgumentNullException ("channels");
			if (users == null)
				throw new ArgumentNullException ("users");

			lock (this.recognition)
			{
				SetupChangeChannelGrammar (channels);
				SetupMoveUserGrammar (channels, users);
			}
		}

		public void Dispose()
		{
			this.recognition.Dispose();
		}

		private SpeechRecognitionEngine recognition;

		private Grammar moveUserGrammar;
		private Grammar changeChannelGrammar;
		private Grammar muteGrammar;

		private void SetupMoveUserGrammar (IEnumerable<IChannelInfo> channels, IEnumerable<IUserInfo> users)
		{
			if (this.moveUserGrammar != null)
				this.recognition.UnloadGrammar (this.moveUserGrammar);
		}

		private void SetupChangeChannelGrammar (IEnumerable<IChannelInfo> channels)
		{
			if (this.changeChannelGrammar != null)
				this.recognition.UnloadGrammar (this.changeChannelGrammar);

			IChannelInfo[] newChannels = channels.ToArray();
			if (newChannels.Length == 0)
				return;

			GrammarBuilder builder = new GrammarBuilder();
			builder.Append (new Choices ("Switch channel", "Switch to channel", "Switch channel to", "Change channel", "Change to channel",
			                             "Change channel to", "Join", "Join channel"));

			Choices channelChoices = new Choices();
			foreach (IChannelInfo channel in newChannels)
				channelChoices.Add (new SemanticResultValue (channel.Name, channel.ChannelId));

			builder.Append (new SemanticResultKey ("changeChannelTo", channelChoices));

			this.changeChannelGrammar = new Grammar (builder);
			this.recognition.LoadGrammar (this.changeChannelGrammar);
		}

		private void OnSpeechRecognized (object sender, SpeechRecognizedEventArgs e)
		{
			foreach (var kvp in e.Result.Semantics)
			{
				switch (kvp.Key)
				{
					case "changeChannelTo":
						OnCommandStateChanged (new CommandStateChangedEventArgs (Command.SwitchChannel, (int)kvp.Value.Value));
						break;

					case "command":
						switch ((string)kvp.Value.Value)
						{
							case "mute":
								OnCommandStateChanged (new CommandStateChangedEventArgs (Command.MuteAll, true));
								break;

							case "unmute":
								OnCommandStateChanged (new CommandStateChangedEventArgs (Command.MuteAll, true));
								break;
						}

						break;
				}
			}
		}

		private void OnCommandStateChanged (CommandStateChangedEventArgs e)
		{
			EventHandler<CommandStateChangedEventArgs> handler = this.CommandStateChanged;
			if (handler != null)
				handler (this, e);
		}
	}
}