using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gablarski.Audio;
using Gablarski.Client;
using Gablarski.Tests.Mocks.Audio;
using NUnit.Framework;

namespace Gablarski.Tests
{
	[TestFixture]
	public class ThreadedAudioEngineTests
	{
		private ICaptureProvider provider;
		private ClientAudioSource source;

		[SetUp]
		public void Setup()
		{
			this.provider = new MockCaptureProvider();
			this.source = new ClientAudioSource (new AudioSource ("mockSource", 1, 1, 1, 64000, 44100, 256, 10, false), new MockClientConnection (new MockServerConnection()));
		}

		[TearDown]
		public void TearDown()
		{
			this.provider = null;
			this.source = null;
		}

		[Test]
		public void InvalidAttach()
		{
			var engine = new ThreadedAudioEngine();

			Assert.Throws<ArgumentNullException> (() => engine.Attach (null, AudioFormat.Mono16Bit, this.source, new AudioEngineCaptureOptions()));
			Assert.Throws<ArgumentNullException> (() => engine.Attach (this.provider, AudioFormat.Mono16Bit, null, new AudioEngineCaptureOptions()));
			Assert.Throws<ArgumentNullException> (() => engine.Attach (this.provider, AudioFormat.Mono16Bit, this.source,  null));
		}

		[Test]
		public void InvalidDetatch()
		{
			var engine = new ThreadedAudioEngine();
			Assert.Throws<ArgumentNullException> (() => engine.Detach ((ICaptureProvider)null));
			Assert.Throws<ArgumentNullException> (() => engine.Detach ((AudioSource)null));
		}

		//[Test]
		//public void AttachDetatchSource()
		//{
		//    var engine = new ThreadedAudioEngine();

		//    engine.Attach (this.provider, this.source, new AudioEngineCaptureOptions());
		//    Assert.IsTrue (engine.Detach (this.source));
		//}

		//[Test]
		//public void AttachDetatchProvider()
		//{
		//    var engine = new ThreadedAudioEngine();

		//    engine.Attach (this.provider, this.source, new AudioEngineCaptureOptions());
		//    Assert.IsTrue (engine.Detach (this.provider));
		//}

		//[Test]
		//public void InvalidBeginCapture()
		//{
		//    var engine = new ThreadedAudioEngine();
		//    engine.Attach (this.provider, this.source, new AudioEngineCaptureOptions());
		//    Assert.Throws<ArgumentNullException> (() => engine.BeginCapture (null));
		//}

		//[Test]
		//public void InvalidEndCapture()
		//{
		//    var engine = new ThreadedAudioEngine();
		//    engine.Attach (this.provider, this.source, new AudioEngineCaptureOptions());
		//    Assert.Throws<ArgumentNullException> (() => engine.EndCapture (null));
		//}
	}
}