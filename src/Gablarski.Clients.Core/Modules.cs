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

using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using Gablarski.Audio;
using Gablarski.Clients.Input;
using Gablarski.Clients.Media;

namespace Gablarski.Clients
{
	public class Modules
	{
		private Modules()
		{
			new CompositionContainer(new DirectoryCatalog (".")).ComposeParts (this);
		}

		public static void Reload()
		{
			modules.Recompose();
		}

		public static IEnumerable<IInputProvider> Input
		{
			get { return modules.input; }
		}

		public static IEnumerable<IAudioPlaybackProvider> Playback
		{
			get { return modules.playback; }
		}

		public static IEnumerable<IAudioCaptureProvider> Capture
		{
			get { return modules.capture; }
		}

		public static IEnumerable<IAudioEngine> AudioEngines
		{
			get { return modules.audioEngines; }
		}

		public static IEnumerable<IMediaPlayer> MediaPlayers
		{
			get { return modules.mediaPlayers; }
		}

		public static IEnumerable<INotifier> Notifiers
		{
			get { return modules.notifiers; }
		}

		public static IEnumerable<ITextToSpeech> TextToSpeech
		{
			get { return modules.tts; }
		}

		public static IEnumerable<ISpeechRecognizer> SpeechRecognizers
		{
			get { return modules.speechRecognizers; }
		}

		private static readonly Modules modules = new Modules();

		[ImportMany (AllowRecomposition = true)]
		private IEnumerable<ITextToSpeech> tts
		{
			get;
			set;
		}

		[ImportMany (AllowRecomposition = true)]
		private IEnumerable<IInputProvider> input
		{
			get;
			set;
		}

		[ImportMany (AllowRecomposition = true)]
		private IEnumerable<IAudioEngine> audioEngines
		{
			get;
			set;
		}

		[ImportMany (AllowRecomposition = true)]
		private IEnumerable<INotifier> notifiers
		{
			get;
			set;
		}

		[ImportMany (AllowRecomposition = true)]
		private IEnumerable<IMediaPlayer> mediaPlayers
		{
			get;
			set;
		}

		[ImportMany (AllowRecomposition = true)]
		private IEnumerable<IAudioCaptureProvider> capture
		{
			get;
			set;
		}

		[ImportMany (AllowRecomposition = true)]
		private IEnumerable<IAudioPlaybackProvider> playback
		{
			get;
			set;
		}

		[ImportMany (AllowRecomposition = true)]
		private IEnumerable<ISpeechRecognizer> speechRecognizers
		{
			get;
			set;
		}

		private void Recompose()
		{
		}
	}
}