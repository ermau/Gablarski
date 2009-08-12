using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gablarski.Audio;
using Gablarski.Tests.Mocks.Audio;
using NUnit.Framework;

namespace Gablarski.Tests
{
	[TestFixture]
	public class AudioEngineTests
	{
		private ICaptureProvider provider;
		private AudioSource source;

		[SetUp]
		public void Setup()
		{
			this.provider = new MockCaptureProvider();
			this.source = new AudioSource ("mockSource", 1, 1, 1, 64000, 44100, 256);
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
			var engine = new AudioEngine();

			Assert.Throws<ArgumentNullException> (() => engine.Attach (null, this.source, new AudioEngineCaptureOptions()));
			Assert.Throws<ArgumentNullException> (() => engine.Attach (this.provider, null, new AudioEngineCaptureOptions()));
			Assert.Throws<ArgumentNullException> (() => engine.Attach (this.provider, this.source,  null));
		}

		[Test]
		public void InvalidDetatch()
		{
			var engine = new AudioEngine();
			Assert.Throws<ArgumentNullException> (() => engine.Detach ((ICaptureProvider)null));
			Assert.Throws<ArgumentNullException> (() => engine.Detach ((AudioSource)null));
		}

		[Test]
		public void AttachDetatch()
		{
			var engine = new AudioEngine();

			engine.Attach (this.provider, this.source, new AudioEngineCaptureOptions());
			Assert.IsTrue (engine.Detach (this.source));

			engine.Attach (this.provider, this.source, new AudioEngineCaptureOptions());
			Assert.IsTrue (engine.Detach (this.provider));
		}
	}
}