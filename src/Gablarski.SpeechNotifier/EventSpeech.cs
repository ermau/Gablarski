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
using System.IO;
using System.Linq;
using System.Speech.AudioFormat;
using System.Text;
using System.Threading;
using Gablarski.Audio;
using Gablarski.Clients;
using System.Speech.Synthesis;
using Gablarski.Clients.Media;

namespace Gablarski.SpeechNotifier
{
	[Export (typeof(ITextToSpeech))]
	public class EventSpeech
		:  ITextToSpeech
	{
		public EventSpeech()
		{
			this.formats = this.speech.Voice.SupportedAudioFormats
				.ToDictionary (af => new AudioFormat (GetWaveEncodingFormat (af.EncodingFormat),
				                                      af.ChannelCount, af.BitsPerSample,
				                                      af.SamplesPerSecond), af => af);
		}

		public string Name
		{
			get { return "SAPI Text to Speech"; }
		}

		public AudioSource AudioSource
		{
			get { return this.audioSource; }
			set
			{
				if (!SupportedFormats.Contains (value))
					throw new ArgumentException ("The audio source's format is unsupported", "value");

				this.audioSource = value;
			}
		}

		public IEnumerable<AudioFormat> SupportedFormats
		{
			get { return this.formats.Keys; }
		}

		public byte[] GetSpeech (string say, AudioSource source)
		{
			if (say == null)
				throw new ArgumentNullException ("say");
			if (source == null)
				throw new ArgumentNullException ("source");

			using (MemoryStream stream = new MemoryStream (120000))
			{
				lock (speech)
				{
					speech.SetOutputToAudioStream (stream, this.formats[source]);
					speech.Speak (say);
				}

				return stream.ToArray();
			}
		}

		public void Say (string say)
		{
			if (say == null)
				throw new ArgumentNullException ("say");
			
			ThreadPool.QueueUserWorkItem (o =>
			{
				lock (sync)
				{
					if (media != null)
						media.AddTalker();

					lock (speech)
						speech.Speak ((string)o);

					if (media != null)
						media.RemoveTalker();
				}
			}, say);
		}

		public IMediaController Media
		{
			get { return media; }

			set
			{
				lock (sync)
					media = value;
			}
		}
		
		public void Notify (NotificationType type, string say, NotifyPriority priority)
		{
			Say (say);
		}
		
		public void Notify (NotificationType type, string say, string nickname, string phonetic, NotifyPriority priority)
		{
			if (say == null)
				throw new ArgumentNullException ("say");
			if (nickname == null)
				throw new ArgumentNullException ("nickname");
			if (phonetic == null)
				throw new ArgumentNullException ("phonetic");

			Notify (type, String.Format (say, phonetic), priority);
		}

		public void Dispose()
		{
			speech.Dispose();
		}

		private readonly object sync = new object();
		private IMediaController media;
		private readonly SpeechSynthesizer speech = new SpeechSynthesizer ();
		private AudioSource audioSource;
		private Dictionary<AudioFormat, SpeechAudioFormatInfo> formats;

		private static WaveFormatEncoding GetWaveEncodingFormat (EncodingFormat encoding)
		{
			switch (encoding)
			{
				case EncodingFormat.Pcm:
					return WaveFormatEncoding.LPCM;

				default:
					return WaveFormatEncoding.Unknown;
			}
		}
	}
}