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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gablarski.Client;
using Cadenza;

namespace Gablarski
{
	public class UserInfo
	{
		internal UserInfo()
		{
		}
		
		internal UserInfo (UserInfo info)
		{
			if (info == null)
				throw new ArgumentNullException ("info");

			this.Nickname = info.Nickname;
			this.Phonetic = info.Phonetic;
			this.Username = info.Username;
			this.UserId = info.UserId;
			this.CurrentChannelId = info.CurrentChannelId;
			this.IsMuted = info.IsMuted;
			this.Status = info.Status;
		}
		
		internal UserInfo (string nickname, string username, int userId, int currentChannelId, bool muted)
			: this (nickname, null, username, userId, currentChannelId, muted)
		{
		}

		internal UserInfo (string nickname, string phonetic, string username, int userId, int currentChannelId, bool muted)
		{
			if (nickname.IsNullOrWhitespace())
				throw new ArgumentNullException ("nickname");
			if (username.IsNullOrWhitespace())
				throw new ArgumentNullException ("username");
			if (userId == 0)
				throw new ArgumentException ("userId");
			if (currentChannelId < 0)
				throw new ArgumentOutOfRangeException ("currentChannelId");

			this.Nickname = nickname;
			this.Phonetic = phonetic;
			this.Username = username;
			this.UserId = userId;
			this.CurrentChannelId = currentChannelId;
			this.IsMuted = muted;
		}

		internal UserInfo (string username, int userId, int currentChannelId, bool muted)
		{
			if (username.IsNullOrWhitespace())
				throw new ArgumentNullException ("username");
			if (userId == 0)
				throw new ArgumentException ("userId");
			if (currentChannelId < 0)
				throw new ArgumentOutOfRangeException ("currentChannelId");

			this.Username = username;
			this.UserId = userId;
			this.CurrentChannelId = currentChannelId;
			this.IsMuted = muted;
		}

		internal UserInfo (IValueReader reader)
		{
			if (reader == null)
				throw new ArgumentNullException("reader");

			Deserialize (reader);
		}

		public int UserId
		{
			get;
			protected set;
		}

		public int CurrentChannelId
		{
			get;
			set;
		}

		public string Username
		{
			get;
			protected set;
		}

		public string Nickname
		{
			get;
			set;
		}
		
		public string Phonetic
		{
			get;
			set;
		}

		public bool IsMuted
		{
			get;
			set;
		}

		public string Status
		{
			get;
			set;
		}

		internal void Serialize (IValueWriter writer)
		{
			writer.WriteInt32 (this.UserId);
			writer.WriteString (this.Username);
			writer.WriteInt32 (this.CurrentChannelId);
			writer.WriteString (this.Nickname);
			writer.WriteString (this.Phonetic);
			writer.WriteBool (this.IsMuted);
			writer.WriteString (this.Status);
		}

		internal void Deserialize (IValueReader reader)
		{
			this.UserId = reader.ReadInt32();
			this.Username = reader.ReadString();
			this.CurrentChannelId = reader.ReadInt32();
			this.Nickname = reader.ReadString();
			this.Phonetic = reader.ReadString();
			this.IsMuted = reader.ReadBool();
			this.Status = reader.ReadString();
		}

		public override bool Equals (object obj)
		{
			var info = (obj as UserInfo);
			if (info != null)
				return Username == info.Username;

			var cu = (obj as CurrentUser);
			if (cu != null)
				return (Username == cu.Username);

			return false;
		}

		public override int GetHashCode ()
		{
			return this.Username.GetHashCode();
		}
	}
}