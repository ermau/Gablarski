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
using Gablarski.Client;
using Cadenza;

namespace Gablarski
{
	[Flags]
	public enum UserStatus
		: byte
	{
		Normal = 0,

		MutedMicrophone = 1,
		MutedSound = 2,
	}

	/// <summary>
	/// Represents a joined user.
	/// </summary>
	public class UserInfo
		: IUser, IEquatable<UserInfo>
	{
		internal UserInfo()
		{
		}
		
		internal UserInfo (UserInfo info)
		{
			if (info == null)
				throw new ArgumentNullException ("info");

			UserId = info.UserId;
			Username = info.Username;
			Nickname = info.Nickname;
			Phonetic = info.Phonetic;
			CurrentChannelId = info.CurrentChannelId;
			IsMuted = info.IsMuted;
			Status = info.Status;
		}
		
		internal UserInfo (string nickname, string username, int userId, int currentChannelId, bool muted)
			: this (nickname, null, username, userId, currentChannelId, muted)
		{
		}

		internal UserInfo (string nickname, string phonetic, UserInfo info)
			: this (nickname, phonetic, info.Username, info.UserId, info.CurrentChannelId, info.IsMuted)
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

			Nickname = nickname;
			Phonetic = phonetic;
			Username = username;
			UserId = userId;
			CurrentChannelId = currentChannelId;
			IsMuted = muted;
		}

		internal UserInfo (string username, int userId, int currentChannelId, bool muted)
		{
			if (username.IsNullOrWhitespace())
				throw new ArgumentNullException ("username");
			if (userId == 0)
				throw new ArgumentException ("userId");
			if (currentChannelId < 0)
				throw new ArgumentOutOfRangeException ("currentChannelId");

			Username = username;
			UserId = userId;
			CurrentChannelId = currentChannelId;
			IsMuted = muted;
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
			set;
		}

		/// <summary>
		/// Gets whether the user is registered or is a guest.
		/// </summary>
		public bool IsRegistered
		{
			get { return (UserId > 0); }
		}

		/// <summary>
		/// Gets the user's unique username. <see cref="Nickname"/> if unregistered.
		/// </summary>
		public string Username
		{
			get;
			set;
		}

		/// <summary>
		/// Gets the Id of the channel the user is currently in.
		/// </summary>
		public int CurrentChannelId
		{
			get;
			set;
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

		public UserStatus Status
		{
			get;
			set;
		}

		public string Comment
		{
			get;
			set;
		}

		internal void Serialize (IValueWriter writer)
		{
			writer.WriteInt32 (UserId);
			writer.WriteString (Username);
			writer.WriteInt32 (CurrentChannelId);
			writer.WriteString (Nickname);
			writer.WriteString (Phonetic);
			writer.WriteBool (IsMuted);
			writer.WriteByte ((byte)Status);
			writer.WriteString (Comment);
		}

		internal void Deserialize (IValueReader reader)
		{
			UserId = reader.ReadInt32();
			Username = reader.ReadString();
			CurrentChannelId = reader.ReadInt32();
			Nickname = reader.ReadString();
			Phonetic = reader.ReadString();
			IsMuted = reader.ReadBool();
			Status = (UserStatus)reader.ReadByte();
			Comment = reader.ReadString();
		}

		public override int GetHashCode()
		{
			return this.Username.GetHashCode();
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

		public bool Equals (UserInfo other)
		{
			if (ReferenceEquals (null, other))
				return false;
			if (ReferenceEquals (this, other))
				return true;

			return Equals (other.Username, this.Username);
		}
	}
}