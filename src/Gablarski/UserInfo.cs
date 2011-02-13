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
		AFK = 4
	}

	/// <summary>
	/// Represents a joined user.
	/// </summary>
	public class UserInfo
		: IUserInfo
	{
		internal UserInfo()
		{
		}
		
		internal UserInfo (IUserInfo info)
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
			Comment = info.Comment;
		}
		
		internal UserInfo (string nickname, string username, int userId, int currentChannelId, bool muted)
			: this (nickname, null, username, userId, currentChannelId, muted)
		{
		}

		internal UserInfo (IUserInfo user, bool muted)
			: this (user.Nickname, user.Phonetic, user.Username, user.UserId, user.CurrentChannelId, muted, user.Comment, user.Status)
		{
		}

		internal UserInfo (IUserInfo user, int currentChannelId)
			: this (user.Nickname, user.Phonetic, user.Username, user.UserId, currentChannelId, user.IsMuted, user.Comment, user.Status)
		{
		}

		internal UserInfo (IUserInfo user, string comment)
			: this (user.Nickname, user.Phonetic, user.Username, user.UserId, user.CurrentChannelId, user.IsMuted, comment, user.Status)
		{
		}

		internal UserInfo (IUserInfo user, UserStatus status)
			: this (user.Nickname, user.Phonetic, user.Username, user.UserId, user.CurrentChannelId, user.IsMuted, user.Comment, status)
		{
		}

		internal UserInfo (string nickname, string phonetic, IUserInfo info)
			: this (nickname, phonetic, info.Username, info.UserId, info.CurrentChannelId, info.IsMuted)
		{
		}

		internal UserInfo (string nickname, string phonetic, string username, int userId, int currentChannelId, bool muted)
			: this(nickname, phonetic, username, userId, currentChannelId, muted, null, UserStatus.Normal)
		{
		}

		internal UserInfo (string nickname, string phonetic, string username, int userId, int currentChannelId, bool muted, string comment, UserStatus status)
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
			Comment = comment;
			Status = status;
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
			protected set;
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
			protected set;
		}

		/// <summary>
		/// Gets the Id of the channel the user is currently in.
		/// </summary>
		public int CurrentChannelId
		{
			get;
			protected set;
		}

		/// <summary>
		/// Gets the user's nickname.
		/// </summary>
		public string Nickname
		{
			get;
			protected set;
		}
		
		/// <summary>
		/// Gets the user's phonetic.
		/// </summary>
		public string Phonetic
		{
			get;
			protected set;
		}

		/// <summary>
		/// Gets whether the user is muted or not.
		/// </summary>
		public bool IsMuted
		{
			get;
			protected set;
		}

		public UserStatus Status
		{
			get;
			protected set;
		}

		public string Comment
		{
			get;
			protected set;
		}

		public void Serialize (IValueWriter writer)
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

		public void Deserialize (IValueReader reader)
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
			var info = (obj as IUserInfo);
			if (info != null)
				return Username == info.Username;

			var cu = (obj as CurrentUser);
			if (cu != null)
				return (Username == cu.Username);

			return false;
		}

		public bool Equals (IUserInfo other)
		{
			if (ReferenceEquals (null, other))
				return false;
			if (ReferenceEquals (this, other))
				return true;

			return Equals (other.Username, this.Username);
		}
	}
}