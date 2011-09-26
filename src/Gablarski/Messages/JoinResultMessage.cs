﻿// Copyright (c) 2011, Eric Maupin
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

using System.Linq;
using Tempest;

namespace Gablarski.Messages
{
	public class JoinResultMessage
		: GablarskiMessage
	{
		public JoinResultMessage()
			: base (GablarskiMessageType.JoinResult)
		{
		}

		public JoinResultMessage (LoginResultState state, IUserInfo user)
			: this()
		{
			this.Result = state;

			if (user != null)
				UserInfo = user;
		}

		public LoginResultState Result
		{
			get; set;
		}

		private UserInfo user;

		public IUserInfo UserInfo
		{
			get { return this.user; }
			set { this.user = new UserInfo (value); }
		}

		#region Overrides of MessageBase

		public override void WritePayload (ISerializationContext context, IValueWriter writer)
		{
			writer.WriteInt32 ((int)this.Result);
			
			if (this.user != null)
				this.user.Serialize (context, writer);
		}

		public override void ReadPayload (ISerializationContext context, IValueReader reader)
		{
			this.Result = (LoginResultState)reader.ReadInt32();

			if (this.Result == LoginResultState.Success)
				this.UserInfo = new UserInfo (context, reader);
		}

		#endregion
	}
}