// Copyright (c) 2009, Eric Maupin
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
using Cadenza;

namespace Gablarski.Messages
{
	public class JoinMessage
		: ClientMessage
	{
		public JoinMessage()
			: base (ClientMessageType.Join)
		{
		}
		
		public JoinMessage (string nickname, string serverPassword)
			: this (nickname, nickname, serverPassword)
		{
		}

		public JoinMessage (string nickname, string phonetic, string serverPassword)
			: this()
		{
			if (nickname.IsNullOrWhitespace())
				throw new ArgumentNullException("nickname");

			this.Nickname = nickname;
			this.Phonetic = phonetic;
			this.ServerPassword = serverPassword;
		}

		public string Nickname
		{
			get; set;
		}
		
		public string Phonetic
		{
			get; set;
		}

		public string ServerPassword
		{
			get; set;
		}

		#region Overrides of MessageBase

		public override void WritePayload (IValueWriter writer)
		{
			writer.WriteString (this.Nickname);
			writer.WriteString (this.Phonetic);
			writer.WriteString (this.ServerPassword);
		}

		public override void ReadPayload (IValueReader reader)
		{
			this.Nickname = reader.ReadString();
			this.Phonetic = reader.ReadString();
			this.ServerPassword = reader.ReadString();
		}

		#endregion
	}
}