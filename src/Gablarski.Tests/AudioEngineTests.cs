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
using System.Linq;
using Gablarski.Audio;
using Gablarski.Client;
using Gablarski.Tests.Mocks.Audio;
using NUnit.Framework;

namespace Gablarski.Tests
{
	[TestFixture]
	public class AudioEngineTests
	{
		private IClientContext context;
		private IAudioReceiver receiver;
		private IAudioSender sender;
		private IAudioCaptureProvider provider;
		private AudioSource source;

		[SetUp]
		public void Setup()
		{
			this.provider = new MockAudioCaptureProvider();
			this.source = new AudioSource ("mockSource", 1, 1, AudioFormat.Mono16bitLPCM, 64000, 256, 10, false);

			var client = new GablarskiClient (new MockClientConnection (new MockConnectionProvider().EstablishConnection()));
			this.context = client;
			this.sender = client.Sources;
			this.receiver = client.Sources;
		}

		[TearDown]
		public void TearDown()
		{
			this.provider = null;
			this.source = null;
			this.receiver = null;
			this.sender = null;
			this.context = null;
		}

		[Test]
		public void InvalidAttach()
		{
			var engine = new AudioEngine();

			Assert.Throws<ArgumentNullException> (() => engine.Attach (null, this.source, new AudioEngineCaptureOptions()));
			Assert.Throws<ArgumentNullException> (() => engine.Attach (this.provider, null, new AudioEngineCaptureOptions()));
			Assert.Throws<ArgumentNullException> (() => engine.Attach (this.provider, this.source, null));
		}

		[Test]
		public void InvalidUpdateCapture()
		{
			var engine = new AudioEngine();

			Assert.Throws<ArgumentNullException> (() => engine.Update (null, new AudioEngineCaptureOptions()));
			Assert.Throws<ArgumentNullException> (() => engine.Update (source, (AudioEngineCaptureOptions)null));
			Assert.Throws<ArgumentException> (() => engine.Update (source, new AudioEngineCaptureOptions()));
		}

		[Test]
		public void InvalidUpdatePlayback()
		{
			var engine = new AudioEngine();

			Assert.Throws<ArgumentNullException> (() => engine.Update (null, new AudioEnginePlaybackOptions()));
			Assert.Throws<ArgumentNullException> (() => engine.Update (source, (AudioEngineCaptureOptions)null));
			Assert.Throws<ArgumentException> (() => engine.Update (source, new AudioEnginePlaybackOptions()));
		}

		[Test]
		public void InvalidUpdateChannelTargets()
		{
			var engine = new AudioEngine();

			Assert.Throws<ArgumentNullException> (() => engine.Update (null, new ChannelInfo[] {}));
			Assert.Throws<ArgumentNullException> (() => engine.Update (source, (IEnumerable<ChannelInfo>)null));
			Assert.Throws<ArgumentException> (() => engine.Update (source, new ChannelInfo[] { }));
		}

		[Test]
		public void InvalidUpdateUserTargets()
		{
			var engine = new AudioEngine();

			Assert.Throws<ArgumentNullException> (() => engine.Update (null, new UserInfo[] {}));
			Assert.Throws<ArgumentNullException> (() => engine.Update (source, (IEnumerable<UserInfo>)null));
			Assert.Throws<ArgumentException> (() => engine.Update (source, new UserInfo[] { }));
		}

		[Test]
		public void InvalidDetatch()
		{
			var engine = new AudioEngine();
			Assert.Throws<ArgumentNullException> (() => engine.Detach ((IAudioCaptureProvider)null));
			Assert.Throws<ArgumentNullException> (() => engine.Detach ((AudioSource)null));
		}

		[Test]
		public void StartWithoutReceiver()
		{
			var engine = new AudioEngine();
			engine.AudioSender = new ClientSourceManager (context);
			engine.Context = context;
			Assert.Throws<InvalidOperationException> (engine.Start);
		}

		[Test]
		public void StartWithoutSender()
		{
			var engine = new AudioEngine();
			engine.AudioReceiver = receiver;
			engine.Context = context;
			Assert.Throws<InvalidOperationException> (engine.Start);
		}

		[Test]
		public void StartWithoutContext()
		{
			var engine = new AudioEngine();
			engine.AudioReceiver = receiver;
			engine.AudioSender = sender;
			Assert.Throws<InvalidOperationException> (engine.Start);
		}

		[Test]
		public void StartRunning()
		{
			var engine = new AudioEngine();
			engine.AudioReceiver = receiver;
			engine.AudioSender = sender;
			engine.Context = context;

			engine.Start();
			Assert.DoesNotThrow (engine.Start);
		}

		[Test]
		public void Start()
		{
			var engine = new AudioEngine();
			engine.AudioReceiver = receiver;
			engine.AudioSender = sender;
			engine.Context = context;
			engine.Start();

			Assert.IsTrue (engine.IsRunning);

			engine.Stop();
		}

		[Test]
		public void Stop()
		{
			var engine = new AudioEngine();
			engine.AudioReceiver = receiver;
			engine.AudioSender = sender;
			engine.Context = context;
			engine.Start();

			engine.Stop();

			Assert.IsFalse (engine.IsRunning);
		}

		[Test]
		public void AttachDetatchSource()
		{
		    var engine = new AudioEngine();

		    engine.Attach (this.provider, this.source, new AudioEngineCaptureOptions());
		    Assert.IsTrue (engine.Detach (this.source));
		}

		[Test]
		public void AttachDetatchProvider()
		{
		    var engine = new AudioEngine();

		    engine.Attach (this.provider, this.source, new AudioEngineCaptureOptions());
		    Assert.IsTrue (engine.Detach (this.provider));
		}

		[Test]
		public void InvalidMute()
		{
			var engine = new AudioEngine();
			Assert.Throws<ArgumentNullException> (() => engine.Mute ((IAudioPlaybackProvider)null));
			Assert.Throws<ArgumentNullException> (() => engine.Mute ((IAudioCaptureProvider)null));
		}

		[Test]
		public void InvalidUnmute()
		{
			var engine = new AudioEngine();
			Assert.Throws<ArgumentNullException> (() => engine.Unmute ((IAudioPlaybackProvider)null));
			Assert.Throws<ArgumentNullException> (() => engine.Unmute ((IAudioCaptureProvider)null));
		}

		//[Test]
		//public void InvalidBeginCapture()
		//{
		//    var engine = new AudioEngine();
		//    engine.Attach (this.provider, AudioFormat.Mono16Bit, this.source, new AudioEngineCaptureOptions());
		//    Assert.Throws<ArgumentNullException> (() => engine.BeginCapture (null));
		//}

		//[Test]
		//public void InvalidEndCapture()
		//{
		//    var engine = new AudioEngine();
		//    engine.Attach (this.provider, AudioFormat.Mono16Bit, this.source, new AudioEngineCaptureOptions());
		//    Assert.Throws<ArgumentNullException> (() => engine.EndCapture (null));
		//}
	}
}