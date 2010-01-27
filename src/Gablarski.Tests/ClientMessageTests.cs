using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Gablarski.Audio;
using Gablarski.Messages;
using NUnit.Framework;
using Gablarski.Client;

namespace Gablarski.Tests
{
	[TestFixture]
	public class ClientMessageTests
	{
		[SetUp]
		public void MessageSetup()
		{
			stream = new MemoryStream(new byte[20480], true);
			writer = new StreamValueWriter (stream);
			reader = new StreamValueReader (stream);
		}

		[TearDown]
		public void MessageTeardown()
		{
			writer.Dispose();
			reader.Dispose();
		}

		private MemoryStream stream;
		private IValueWriter writer;
		private IValueReader reader;

		[Test]
		public void Connect()
		{
			var msg = new ConnectMessage { ProtocolVersion = 42, Host = "monkeys.com", Port = 6112 };

			msg.WritePayload (writer);
			long length = stream.Position;
			stream.Position = 0;

			msg = new ConnectMessage();
			msg.ReadPayload (reader);

			Assert.AreEqual (length, stream.Position);
			Assert.AreEqual (42, msg.ProtocolVersion);
			Assert.AreEqual ("monkeys.com", msg.Host);
			Assert.AreEqual (6112, msg.Port);
		}

		[Test]
		public void Join()
		{
			string nickname = "Foo";
			var msg = new JoinMessage { Nickname = nickname };
			msg.WritePayload (writer);
			long length = stream.Position;
			stream.Position = 0;

			msg = new JoinMessage();
			msg.ReadPayload (reader);

			Assert.AreEqual (length, stream.Position);
			Assert.AreEqual (nickname, msg.Nickname);
		}
		
		[Test]
		public void JoinWithServerPassword()
		{
			string nickname = "Foo";
			string password = "pass";
			var msg = new JoinMessage { Nickname = nickname, ServerPassword = password };
			Assert.AreEqual (nickname, msg.Nickname);
			Assert.AreEqual (password, msg.ServerPassword);
			msg.WritePayload (writer);
			long length = stream.Position;
			stream.Position = 0;

			msg = new JoinMessage();
			msg.ReadPayload (reader);

			Assert.AreEqual (length, stream.Position);
			Assert.AreEqual (nickname, msg.Nickname);
			Assert.AreEqual (password, msg.ServerPassword);
		}
		
		[Test]
		public void JoinWithPhonetic ()
		{
			string nickname = "Foo";
			string password = "pass";
			string phonetic = "Phoo";
			var msg = new JoinMessage (nickname, phonetic, password);
			Assert.AreEqual (nickname, msg.Nickname);
			Assert.AreEqual (phonetic, msg.Phonetic);
			Assert.AreEqual (password, msg.ServerPassword);
			msg.WritePayload (writer);
			long length = stream.Position;
			stream.Position = 0;
			
			msg = new JoinMessage ();
			msg.ReadPayload (reader);
			
			Assert.AreEqual (length, stream.Position);
			Assert.AreEqual (nickname, msg.Nickname);
			Assert.AreEqual (phonetic, msg.Phonetic);
			Assert.AreEqual (password, msg.ServerPassword);
		}

		[Test]
		public void Login()
		{
			string username = "foo_";
			string password = "monkeys";

			var msg = new LoginMessage { Username = username, Password = password };
			msg.WritePayload (writer);
			long length = stream.Position;
			stream.Position = 0;

			msg = new LoginMessage();
			msg.ReadPayload (reader);

			Assert.AreEqual (length, stream.Position);
			Assert.AreEqual (username, msg.Username);
			Assert.AreEqual (password, msg.Password);
		}

		[Test]
		public void RequestSource()
		{
			const string name = "Voice";
			const int bitrate = 64000;
			const short frameSize = 512;
			const byte channels = 1;
			const int frequency = 44100;
			const byte complexity = 10;

			var msg = new RequestSourceMessage (name, new AudioCodecArgs (channels, bitrate, frequency, frameSize, complexity));
			Assert.AreEqual (name, msg.Name);
			msg.WritePayload (writer);
			long length = stream.Position;
			stream.Position = 0;

			msg = new RequestSourceMessage();
			msg.ReadPayload (reader);
			Assert.AreEqual (length, stream.Position);
			Assert.AreEqual (name, msg.Name);
		}

		[Test]
		public void SendAudioData()
		{
			var msg = new SendAudioDataMessage
			{
				SourceId = 1,
				TargetType = TargetType.User,
				TargetIds = new[] { 2 },
				Data = new byte[] { 0x4, 0x8, 0xF, 0x10, 0x17, 0x2A }
			};

			Assert.AreEqual (new[] { 2 }, msg.TargetIds);
			Assert.AreEqual (TargetType.User, msg.TargetType);
			Assert.AreEqual (1, msg.SourceId);
			Assert.AreEqual (0x4, msg.Data[0]);
			Assert.AreEqual (0x8, msg.Data[1]);
			Assert.AreEqual (0xF, msg.Data[2]);
			Assert.AreEqual (0x10, msg.Data[3]);
			Assert.AreEqual (0x17, msg.Data[4]);
			Assert.AreEqual (0x2A, msg.Data[5]);

			msg.WritePayload (writer);
			long length = stream.Position;
			stream.Position = 0;

			msg = new SendAudioDataMessage();
			msg.ReadPayload (reader);
			Assert.AreEqual (length, stream.Position);
			Assert.AreEqual (new[] { 2 }, msg.TargetIds);
			Assert.AreEqual (TargetType.User, msg.TargetType);
			Assert.AreEqual (1, msg.SourceId);
			Assert.AreEqual (0x4, msg.Data[0]);
			Assert.AreEqual (0x8, msg.Data[1]);
			Assert.AreEqual (0xF, msg.Data[2]);
			Assert.AreEqual (0x10, msg.Data[3]);
			Assert.AreEqual (0x17, msg.Data[4]);
			Assert.AreEqual (0x2A, msg.Data[5]);
		}

		[Test]
		public void RequestMuteUser()
		{
			Assert.Throws<ArgumentNullException> (() => new RequestMuteUserMessage (null, true));

			var user = new UserInfo ("Nick", 1, 2, true);

			var msg = new RequestMuteUserMessage (user, false);

			msg.WritePayload (writer);
			long length = stream.Position;
			stream.Position = 0;

			msg = new RequestMuteUserMessage();
			msg.ReadPayload (reader);
			Assert.AreEqual (length, stream.Position);
			Assert.AreEqual (user.UserId, msg.TargetId);
			Assert.AreEqual (true, msg.Unmute);
		}

		[Test]
		public void RequestMuteSource()
		{
			Assert.Throws<ArgumentNullException> (() => new RequestMuteSourceMessage (null, true));

			var msg = new RequestMuteSourceMessage (new AudioSource ("Name", 5, 2, 1, 64000, 44100, 512, 10, false), false);
			msg.WritePayload (writer);
			long length = stream.Position;
			stream.Position = 0;

			msg = new RequestMuteSourceMessage();
			msg.ReadPayload (reader);
			Assert.AreEqual (length, stream.Position);
			Assert.AreEqual (5, msg.TargetId);
			Assert.AreEqual (true, msg.Unmute);
		}

		[Test]
		public void QueryServer()
		{
			var msg = new QueryServerMessage { ServerInfoOnly = true };
			msg.WritePayload (writer);
			long length = stream.Position;
			stream.Position = 0;

			msg = new QueryServerMessage();
			msg.ReadPayload (reader);
			Assert.AreEqual (length, stream.Position);
			Assert.AreEqual (true, msg.ServerInfoOnly);
		}

		[Test]
		public void RegisterInvalid()
		{
			Assert.Throws<ArgumentNullException> (() => new RegisterMessage (null, "Password"));
			Assert.Throws<ArgumentException> (() => new RegisterMessage (String.Empty, "Password"));

			Assert.Throws<ArgumentNullException> (() => new RegisterMessage ("Username", null));
			Assert.Throws<ArgumentException> (() => new RegisterMessage ("Username", String.Empty));
		}

		[Test]
		public void Register()
		{
			var msg = new RegisterMessage ("Username", "Password");
			Assert.AreEqual ("Username", msg.Username);
			Assert.AreEqual ("Password", msg.Password);

			msg = AssertLengthMatches (msg, () => new RegisterMessage());
			Assert.AreEqual ("Username", msg.Username);
			Assert.AreEqual ("Password", msg.Password);
		}

		private T AssertLengthMatches<T> (T msg, Func<T> newMessage)
			where T : MessageBase
		{
			msg.WritePayload (writer);
			long len = stream.Position;
			stream.Position = 0;

			msg = newMessage();
			msg.ReadPayload (reader);
			Assert.AreEqual (len, stream.Position);

			return msg;
		}
	}
}