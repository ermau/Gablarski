//
// UserViewModel.cs
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
using System.Linq;
using System.Windows.Input;
using Gablarski.Audio;
using Gablarski.Client;
using Gablarski.Clients.Messages;

namespace Gablarski.Clients.ViewModels
{
	public class UserViewModel
		: ViewModelBase
	{
		public UserViewModel (IGablarskiClientContext context, IUserInfo user)
		{
			if (context == null)
				throw new ArgumentNullException ("context");
			if (user == null)
				throw new ArgumentNullException ("user");

			this.context = context;
			this.context.Sources.AudioSourceStarted += OnAudioSourceStarted;
			this.context.Sources.AudioSourceStopped += OnAudioSourceStopped;
			this.context.Users.UserMuted += OnUserMuted;

			IsIgnored = this.context.Sources.GetSources (user).Any (s => this.context.Sources.GetIsIgnored (s));

			User = user;

			ToggleIgnoreUser = new RelayCommand (() => IsIgnored = !isIgnored);

			var request = new GetUserGainMessage (user);
			Messenger.Send (request);
			this.volume = (1 + Math.Log (request.Gain + .1)) * (1 / (1 + Math.Log (1.1)));
		}

		public IUserInfo User
		{
			get;
			private set;
		}

		public bool IsHearable
		{
			get { return (!IsIgnored && !IsMuted); }
		}

		public bool IsTalking
		{
			get { return this.isTalking; }
			private set
			{
				if (this.isTalking == value)
					return;

				this.isTalking = value;
				OnPropertyChanged();
			}
		}

		public ICommand ToggleIgnoreUser
		{
			get;
			private set;
		}

		public bool IsMuted
		{
			get { return User.IsMuted; }
		}

		public bool IsIgnored
		{
			get { return this.isIgnored; }
			private set
			{
				if (this.isIgnored == value)
					return;

				this.isIgnored = value;
				OnPropertyChanged();
				OnPropertyChanged ("IsHearable");

				Messenger.Send (new IgnoreUserMessage (User, value));
			}
		}

		public double Volume
		{
			get { return this.volume; }
			set
			{
				if (this.volume == value)
					return;

				this.volume = value;
				OnPropertyChanged();

				float gain = (float) ((Math.Pow (10, (value - 1)) - 0.1) * (9d / 10d));

				AudioSource source = this.context.Sources.GetSources (User).FirstOrDefault();
				if (source != null)
					this.context.Audio.Update (source, new AudioEnginePlaybackOptions (gain));

				Messenger.Send (new AdjustUserGainMessage (User, value));
			}
		}

		private readonly IGablarskiClientContext context;
		private bool isTalking;

		private readonly HashSet<int> sourcesPlaying = new HashSet<int>();
		private double volume;
		private bool isIgnored;

		private void OnUserMuted(object sender, UserMutedEventArgs e)
		{
			if (!Equals (e.User, this.User))
				return;

			OnPropertyChanged ("IsMuted");
			OnPropertyChanged ("IsHearable");
		}

		private void OnAudioSourceStopped (object sender, AudioSourceEventArgs e)
		{
			if (e.Source.OwnerId != User.UserId)
				return;

			lock (this.sourcesPlaying) {
				this.sourcesPlaying.Remove (e.Source.Id);
				if (this.sourcesPlaying.Count == 0)
					IsTalking = false;
			}
		}

		private void OnAudioSourceStarted (object sender, AudioSourceEventArgs e)
		{
			if (e.Source.OwnerId != User.UserId)
				return;

			lock (this.sourcesPlaying) {
				if (this.sourcesPlaying.Contains (e.Source.Id))
					return;

				this.sourcesPlaying.Add (e.Source.Id);
				IsTalking = true;
			}
		}
	}
}
