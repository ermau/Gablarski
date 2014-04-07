// Copyright (c) 2014, Eric Maupin
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
using System.Threading;
using Gablarski.Audio;
using Gablarski.Client;

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

			User = user;
		}

		public IUserInfo User
		{
			get;
			private set;
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

		private readonly IGablarskiClientContext context;
		private bool isTalking;

		private int sourcesPlaying;

		private void OnAudioSourceStopped (object sender, AudioSourceEventArgs e)
		{
			if (e.Source.OwnerId != User.UserId)
				return;

			int newValue = Interlocked.Decrement (ref this.sourcesPlaying);
			if (newValue == 0)
				IsTalking = false;
		}

		private void OnAudioSourceStarted (object sender, AudioSourceEventArgs e)
		{
			if (e.Source.OwnerId != User.UserId)
				return;

			Interlocked.Increment (ref this.sourcesPlaying);
			IsTalking = true;
		}
	}
}
