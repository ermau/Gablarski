//
// InputHandler.cs
//
// Author:
//   Eric Maupin <me@ermau.com>
//
// Copyright (c) 2014, Xamarin Inc.
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
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Gablarski.Audio;
using Gablarski.Client;
using Gablarski.Clients.Input;

namespace Gablarski.Clients
{
	/// <summary>
	/// A manager for input, handles setting up/tearing down input providers and handles <see cref="Command" />s.
	/// </summary>
	/// <remarks>At current, the manager can not handle a changing list of input providers.</remarks>
	public sealed class InputHandler
	{
		/// <summary>
		/// Creates and initializes a new instance of the <see cref="InputHandler"/> class.
		/// </summary>
		/// <param name="context">The client context.</param>
		public InputHandler (IGablarskiClientContext context)
		{
			if (context == null)
				throw new ArgumentNullException ("context");

			this.context = context;
			Settings.SettingChanged += OnSettingChanged;
		}

		/// <summary>
		/// Gets or sets the <see cref="AudioSource" /> to send audio from on the <see cref="Command.Talk" /> command.
		/// </summary>
		public AudioSource VoiceSource
		{
			get;
			set;
		}

		public Task SetupInputAsync (IntPtr handle)
		{
			Task input = SetupInputProvider (handle);
			Task speech = SetupSpeechRecognizer();
			return Task.WhenAll (input, speech);
		}

		public void DisableInput()
		{
			DisableInputProvider();
			DisableSpeechRecognizer();
		}

		private IntPtr inputWindowHandle;
		private readonly IGablarskiClientContext context;

		private IInputProvider InputProvider
		{
			get;
			set;
		}

		private string InputProviderTypeName
		{
			get { return Settings.InputProvider; }
		}

		private ISpeechRecognizer SpeechRecognizer
		{
			get;
			set;
		}

		private IAudioEngine Audio
		{
			get { return this.context.Audio; }
		}

		private bool PushToTalk
		{
			get { return Settings.UsePushToTalk; }
		}

		private async Task SetupSpeechRecognizer()
		{
			SpeechRecognizer = await Modules.GetImplementerOrDefaultAsync<ISpeechRecognizer> (null).ConfigureAwait (false);
			if (SpeechRecognizer == null)
				return;

			await SpeechRecognizer.OpenAsync().ConfigureAwait (false);
			SpeechRecognizer.CommandStateChanged += OnCommandStateChanged;
		}

		private void DisableSpeechRecognizer()
		{
			var recognizer = SpeechRecognizer;
			SpeechRecognizer = null;

			if (recognizer == null)
				return;

			recognizer.CommandStateChanged -= OnCommandStateChanged;
			recognizer.Close();
		}

		private async Task SetupInputProvider (IntPtr handle)
		{
			this.inputWindowHandle = handle;

			InputProvider = await Modules.GetImplementerAsync<IInputProvider> (InputProviderTypeName).ConfigureAwait (false);

			if (InputProvider == null)
				throw new InvalidOperationException ("An input provider could not be found");

			InputProvider.CommandStateChanged += OnCommandStateChanged;
			await InputProvider.AttachAsync (handle).ConfigureAwait (false);

			string name = InputProvider.GetType().GetSimpleName();
			InputProvider.SetBindings (Settings.CommandBindings.Where (b => b.Provider.GetType().GetSimpleName() == name));
		}

		private void DisableInputProvider()
		{
			var input = InputProvider;
			InputProvider = null;

			if (input == null)
				return;

			input.CommandStateChanged -= OnCommandStateChanged;
			input.Detach();
		}

		private async void OnSettingChanged (object sender, PropertyChangedEventArgs e)
		{
			switch (e.PropertyName) {
				case Settings.InputProviderSettingName:
					DisableInput();
					await SetupInputAsync (this.inputWindowHandle);
					break;
			}
		}

		private void OnCommandStateChanged (object sender, CommandStateChangedEventArgs e)
		{
			switch (e.Command) {
				case Command.ActivateRecognition:
					ActivateRecognition ((bool)e.State);
					break;

				case Command.Talk:
					Talk ((bool) e.State);
					break;

				case Command.SwitchChannel:
					SwitchChannel (e.State);
					break;
			}
		}

		private void SwitchChannel (object state)
		{
			IChannelInfo channel = state as IChannelInfo;
			if (channel == null) {
				if (state is int)
					channel = this.context.Channels[(int) state];
			}

			if (channel == null)
				return;

			this.context.Users.MoveAsync (this.context.CurrentUser, channel);
		}

		private void Talk (bool on)
		{
			if (PushToTalk || VoiceSource == null)
				return;

			if (on)
				Audio.BeginCapture (VoiceSource, this.context.GetCurrentChannel());
			else
				Audio.EndCapture (VoiceSource);
		}

		private void ActivateRecognition (bool on)
		{
			if (SpeechRecognizer == null)
				return;

			if (!PushToTalk) {
				if (on)
					Audio.MuteCapture();
				else
					Audio.UnmuteCapture();
			}

			if (on)
				SpeechRecognizer.StartRecognizing();
			else
				SpeechRecognizer.StopRecognizing();
		}
	}
}