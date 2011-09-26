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

using System;
using System.Linq;
using Cadenza;
using Tempest;

namespace Gablarski.Messages
{
	public class RegisterMessage
		: GablarskiMessage
	{
		public RegisterMessage()
			: base (GablarskiMessageType.Register)
		{
		}

		public RegisterMessage (string username, string password)
			: base (GablarskiMessageType.Register)
		{
			if (username == null)
				throw new ArgumentNullException ("username");
			if (username.IsNullOrWhitespace())
				throw new ArgumentException ("username");
			if (password == null)
				throw new ArgumentNullException ("password");
			if (password.IsNullOrWhitespace())
				throw new ArgumentException ("password");

			Username = username;
			Password = password;
		}

		public override bool Encrypted
		{
			get { return true; }
		}

		public string Username
		{
			get;
			private set;
		}

		public string Password
		{
			get;
			private set;
		}

		public override void WritePayload (ISerializationContext context, IValueWriter writer)
		{
			writer.WriteString (Username);
			writer.WriteString (Password);
		}

		public override void ReadPayload (ISerializationContext context,IValueReader reader)
		{
			Username = reader.ReadString();
			Password = reader.ReadString();
		}
	}
}