//
// ServerViewModel.cs
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
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using Gablarski.Audio;
using Gablarski.Client;
using Gablarski.Clients.Messages;
using Gablarski.Clients.Persistence;
using Tempest;

namespace Gablarski.Clients.ViewModels
{
	public class ServerViewModel
		: ViewModelBase
	{
		public ServerViewModel (IGablarskiClientContext clientContext, IntPtr windowHandle)
		{
			if (clientContext == null)
				throw new ArgumentNullException ("clientContext");

			this.clientContext = clientContext;
			this.input = new InputHandler (this.clientContext);
			this.audio = new AudioHandler (this.clientContext);

			this.windowHandle = windowHandle;

			BindingOperations.EnableCollectionSynchronization (this.clientContext.Channels, this.clientContext.Channels.SyncContext);

			Channels = new CollectionView<ChannelViewModel> (this.clientContext.Channels,
				new LambdaConverter<IChannelInfo, ChannelViewModel> (
					c => new ChannelViewModel (this.clientContext, c), vm => vm.Channel));

			LeaveServer = new RelayCommand (OnLeave);
			MuteMicrophone = new RelayCommand (OnMuteMicrophone);
			MuteSound = new RelayCommand (OnMuteSound);
		}

		public ServerInfo ServerInfo
		{
			get { return this.clientContext.ServerInfo; }
		}

		public IEnumerable<ChannelViewModel> Channels
		{
			get;
			private set;
		}

		public ICommand LeaveServer
		{
			get;
			private set;
		}

		public ICommand MuteMicrophone
		{
			get;
			private set;
		}

		public ICommand MuteSound
		{
			get;
			private set;
		}

		public bool IsConnecting
		{
			get { return this.isConnecting; }
			private set
			{
				if (this.isConnecting == value)
					return;

				this.isConnecting = value; 
				OnPropertyChanged();
			}
		}

		public ClientConnectionResult ConnectionResult
		{
			get { return this.connectionResult; }
			private set
			{
				if (this.connectionResult == value)
					return;

				this.connectionResult = value;
				OnPropertyChanged();
			}
		}

		public async Task JoinAsync (ServerEntry entry)
		{
			if (entry == null)
				throw new ArgumentNullException ("entry");

			IsConnecting = true;

			var target = new Target (entry.Host, entry.Port);
			ConnectionResult = await this.clientContext.ConnectAsync (target);

			if (ConnectionResult.Result != Tempest.ConnectionResult.Success)
				return;

			OnPropertyChanged ("ServerInfo");

			Task<LoginResultState> joinTask = this.clientContext.CurrentUser.JoinAsync (entry.UserNickname, entry.UserPhonetic, entry.ServerPassword);

			Task inputTask = this.input.SetupInputAsync (this.windowHandle);
			
			await this.audio.SetupAudioAsync();

			// TODO failed joins
			await joinTask;

			// TODO failed request
			Task<AudioSource> voiceSourceTask = this.audio.RequestVoiceSourceAsync (new AudioCodecArgs (AudioFormat.Mono16bitLPCM, 0, 480, 10));

			await inputTask;
			this.input.VoiceSource = await voiceSourceTask;

			IsConnecting = false;
		}

		private readonly IGablarskiClientContext clientContext;
		private readonly IntPtr windowHandle;
		private bool isConnecting;
		private ClientConnectionResult connectionResult;
		private InputHandler input;
		private AudioHandler audio;

		private void OnMuteSound()
		{
			throw new NotImplementedException();
		}

		private void OnMuteMicrophone()
		{
			throw new NotImplementedException();
		}

		private async void OnLeave()
		{
			var i = this.input;
			if (i != null)
				i.DisableInput();

			var a = this.audio;
			if (a != null)
				a.DisableAudio();

			await this.clientContext.DisconnectAsync();
			Messenger.Send (new LeaveServerMessage());
		}
	}
}