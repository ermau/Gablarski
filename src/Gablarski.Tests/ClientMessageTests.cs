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
using System.IO;
using System.Linq;
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
			string password = "pass";

			var msg = new LoginMessage { Username = username, Password = password };
			Assert.AreEqual (username.Length + password.Length, msg.MessageSize);
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

			var msg = new RequestSourceMessage (name, new AudioCodecArgs (AudioFormat.Mono16Bit, bitrate, frequency, frameSize, complexity));
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
			var msg = new ClientAudioDataMessage
			{
				SourceId = 1,
				TargetType = TargetType.User,
				TargetIds = new[] { 2 },
				Sequence = 20,
				Data = new [] { new byte[] { 0x4, 0x8, 0xF, 0x10, 0x17, 0x2A } }
			};

			msg.WritePayload (writer);
			long length = stream.Position;
			stream.Position = 0;

			msg = new ClientAudioDataMessage();
			msg.ReadPayload (reader);
			Assert.AreEqual (length, stream.Position);
			Assert.AreEqual (new[] { 2 }, msg.TargetIds);
			Assert.AreEqual (TargetType.User, msg.TargetType);
			Assert.AreEqual (20, msg.Sequence);
			Assert.AreEqual (1, msg.SourceId);
			Assert.AreEqual (1, msg.Data.Length);
			Assert.AreEqual (0x4, msg.Data[0][0]);
			Assert.AreEqual (0x8, msg.Data[0][1]);
			Assert.AreEqual (0xF, msg.Data[0][2]);
			Assert.AreEqual (0x10, msg.Data[0][3]);
			Assert.AreEqual (0x17, msg.Data[0][4]);
			Assert.AreEqual (0x2A, msg.Data[0][5]);
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

			var msg = new RequestMuteSourceMessage (new AudioSource ("Name", 5, 2, AudioFormat.Mono16Bit, 64000, 44100, 512, 10, false), false);
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

			msg = AssertLengthMatches (msg);
			Assert.AreEqual ("Username", msg.Username);
			Assert.AreEqual ("Password", msg.Password);
		}

		[Test]
		public void SetComment()
		{
			var msg = new SetCommentMessage ("Comment");
			Assert.AreEqual ("Comment", msg.Comment);

			msg = AssertLengthMatches (msg);
			Assert.AreEqual ("Comment", msg.Comment);
		}

		[Test]
		public void SetStatus()
		{
			var msg = new SetStatusMessage (UserStatus.MutedMicrophone);
			Assert.AreEqual (UserStatus.MutedMicrophone, msg.Status);

			msg = AssertLengthMatches (msg);
			Assert.AreEqual (UserStatus.MutedMicrophone, msg.Status);
		}

		private T AssertLengthMatches<T> (T msg)
			where T : MessageBase, new()
		{
			msg.WritePayload (writer);
			long len = stream.Position;
			stream.Position = 0;

			msg = new T();
			msg.ReadPayload (reader);
			Assert.AreEqual (len, stream.Position);

			return msg;
		}
	}
}