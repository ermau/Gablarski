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
using System.Text;
using Gablarski.Client;
using NUnit.Framework;

namespace Gablarski.Tests
{
	[TestFixture]
	public class ClientUserManagerTests
	{
		[Test]
		public void Join ()
		{
			var user = new UserInfo ("Nickname", "Phonetic", "Username", 1, 2, true);

			var manager = new ClientUserManager ();
			manager.Join (user);

			Assert.IsTrue (manager.GetIsJoined (user));
			Assert.IsTrue (((IEnumerable<UserInfo>)manager).Contains (user));
			Assert.AreEqual (user, manager[user.UserId]);
		}
		
		[Test]
		public void JoinNullUser ()
		{
			var manager = new ClientUserManager ();
			Assert.Throws<ArgumentNullException> (() => manager.Join (null));
		}
		
		[Test]
		public void Depart()
		{
			var user = new UserInfo ("Nickname", "Phonetic", "Username", 1, 2, true);
			
			var manager = new ClientUserManager();
			manager.Join (user);
			
			Assert.IsTrue (manager.GetIsJoined (user));
			Assert.IsTrue (((IEnumerable<UserInfo>)manager).Contains (user));
			Assert.AreEqual (user, manager[user.UserId]);
			
			Assert.IsTrue (manager.Depart (user));
			
			Assert.IsFalse (manager.GetIsJoined (user));
			Assert.IsFalse (((IEnumerable<UserInfo>)manager).Contains (user));
			Assert.AreEqual (null, manager[user.UserId]);
		}
		
		[Test]
		public void DepartNonJoined()
		{
			var user = new UserInfo ("Nickname", "Phonetic", "Username", 1, 2, true);
			
			var manager = new ClientUserManager();
			
			Assert.IsFalse (manager.GetIsJoined (user));
			Assert.IsFalse (manager.Depart (user));
		}
		
		[Test]
		public void DepartNullUser()
		{
			var manager = new ClientUserManager();
			Assert.Throws<ArgumentNullException> (() => manager.Depart (null));
		}
		
		[Test]
		public void UpdateNullUsers()
		{
			var manager = new ClientUserManager();
			Assert.Throws<ArgumentNullException> (() => manager.Update ((IEnumerable<UserInfo>)null));
		}
		
		[Test]
		public void UpdateEmpty()
		{
			var user = new UserInfo ("Nickname", "Phonetic", "Username", 1, 2, true);
			var user2 = new UserInfo ("Nickname2", "Phonetic2", "Username2", 2, 3, false);
			
			var manager = new ClientUserManager();
			
			manager.Update (new [] { user, user2 });
			
			Assert.IsTrue (manager.GetIsJoined (user));
			Assert.IsTrue (manager.GetIsJoined (user2));
		}
		
		[Test]
		public void Update()
		{
			var user = new UserInfo ("Nickname", "Phonetic", "Username", 1, 2, true);
			
			var manager = new ClientUserManager();
			manager.Join (user);
			
			user = new UserInfo ("NicknameU", "PhoneticU", "Username2", 1, 2, false);
			var user2 = new UserInfo ("Nickname2U", "PhoneticU", "Username2U", 2, 3, false);
			
			manager.Update (new [] { user, user2 });
			
			Assert.IsTrue (manager.GetIsJoined (user));
			Assert.AreEqual (user, manager[user.UserId]);
			Assert.AreEqual (user.Nickname, manager[user.UserId].Nickname);
			Assert.AreEqual (user.Phonetic, manager[user.UserId].Phonetic);
			Assert.AreEqual (user.Username, manager[user.UserId].Username);
			Assert.AreEqual (user.UserId, manager[user.UserId].UserId);
			Assert.AreEqual (user.CurrentChannelId, manager[user.UserId].CurrentChannelId);
			Assert.AreEqual (user.IsMuted, manager[user.UserId].IsMuted);
			
			Assert.IsTrue (manager.GetIsJoined (user2));
			Assert.AreEqual (user2, manager[user2.UserId]);
			Assert.AreEqual (user2.Nickname, manager[user2.UserId].Nickname);
			Assert.AreEqual (user2.Phonetic, manager[user2.UserId].Phonetic);
			Assert.AreEqual (user2.Username, manager[user2.UserId].Username);
			Assert.AreEqual (user2.UserId, manager[user2.UserId].UserId);
			Assert.AreEqual (user2.CurrentChannelId, manager[user2.UserId].CurrentChannelId);
			Assert.AreEqual (user2.IsMuted, manager[user2.UserId].IsMuted);
		}
		
		[Test]
		public void UpdateNullUser()
		{
			var manager = new ClientUserManager();
			Assert.Throws<ArgumentNullException> (() => manager.Update ((UserInfo)null));
		}
		
		[Test]
		public void UpdateUser()
		{
			var user = new UserInfo ("Nickname", "Phonetic", "Username", 1, 2, true);
			var user2 = new UserInfo ("Nickname2", "Phonetic2", "Username2", 2, 3, false);
			
			var manager = new ClientUserManager();
			manager.Join (user);
			
			user = new UserInfo ("NicknameU", "PhoneticU", "Username2", 1, 2, false);
			user2 = new UserInfo ("Nickname2U", "Phonetic2U", "Username2", 2, 3, false);
			
			manager.Update (user);
			
			Assert.IsTrue (manager.GetIsJoined (user));
			Assert.AreEqual (user, manager[user.UserId]);
			Assert.AreEqual (user.Nickname, manager[user.UserId].Nickname);
			Assert.AreEqual (user.Phonetic, manager[user.UserId].Phonetic);
			Assert.AreEqual (user.Username, manager[user.UserId].Username);
			Assert.AreEqual (user.UserId, manager[user.UserId].UserId);
			Assert.AreEqual (user.CurrentChannelId, manager[user.UserId].CurrentChannelId);
			Assert.AreEqual (user.IsMuted, manager[user.UserId].IsMuted);
			
			manager.Update (user2);
			
			Assert.IsTrue (manager.GetIsJoined (user2));
			Assert.AreEqual (user2, manager[user2.UserId]);
			Assert.AreEqual (user2.Nickname, manager[user2.UserId].Nickname);
			Assert.AreEqual (user2.Phonetic, manager[user2.UserId].Phonetic);
			Assert.AreEqual (user2.Username, manager[user2.UserId].Username);
			Assert.AreEqual (user2.UserId, manager[user2.UserId].UserId);
			Assert.AreEqual (user2.CurrentChannelId, manager[user2.UserId].CurrentChannelId);
			Assert.AreEqual (user2.IsMuted, manager[user2.UserId].IsMuted);
		}

		[Test]
		public void GetIsJoined()
		{
			var user = new UserInfo ("Nickname", "Phonetic", "Username", 1, 2, true);
			var user2 = new UserInfo ("Nickname2", "Phonetic", "Username2", 2, 3, false);
			
			var manager = new ClientUserManager();
			manager.Join (user);
			
			Assert.IsTrue (manager.GetIsJoined (user));
			Assert.IsFalse (manager.GetIsJoined (user2));
			
			manager.Join (user2);
			Assert.IsTrue (manager.GetIsJoined (user2));
			
			manager.Depart (user);
			Assert.IsFalse (manager.GetIsJoined (user));
		}
		
		[Test]
		public void ExtensionGetIsJoinedId()
		{
			var user = new UserInfo ("Nickname", "Username", 1, 2, true);
			var user2 = new UserInfo ("Nickname2", "Username2", 2, 3, false);
			
			var manager = new ClientUserManager();
			manager.Join (user);
			
			Assert.IsTrue (manager.GetIsJoined (user));
			Assert.IsFalse (manager.GetIsJoined (user2));
			
			manager.Join (user2);
			Assert.IsTrue (manager.GetIsJoined (user2));
			
			manager.Depart (user);
			Assert.IsFalse (manager.GetIsJoined (user));
		}

		[Test]
		public void ExtensionGetIsJoinedUsername()
		{
			var user = new UserInfo ("Nickname", "Phonetic", "Username", 1, 2, true);
			var user2 = new UserInfo ("Nickname2", "Phonetic", "Username2", 2, 3, false);
			
			var manager = new ClientUserManager();
			manager.Join (user);
			
			Assert.IsTrue (manager.GetIsJoined (user));
			Assert.IsFalse (manager.GetIsJoined (user2));
			
			manager.Join (user2);
			Assert.IsTrue (manager.GetIsJoined (user2));

			manager.Depart (user);
			Assert.IsFalse (manager.GetIsJoined (user));
		}
		
		[Test]
		public void GetIsJoinedNullUser ()
		{
			var manager = new ClientUserManager ();
			Assert.Throws<ArgumentNullException> (() => manager.GetIsJoined ((UserInfo)null));
		}
		
		[Test]
		public void IndexerNotPresent()
		{
			var user = new UserInfo ("Nickname", "Phonetic", "Username", 1, 2, true);
			var user2 = new UserInfo ("Nickname2", "Phonetic2", "Username2", 2, 3, false);
			
			var manager = new ClientUserManager();
			manager.Join (user);
			
			Assert.AreEqual (null, manager[user2.UserId]);
		}
		
		[Test]
		public void IndexerPresent()
		{
			var user = new UserInfo ("Nickname", "Phonetic", "Username", 1, 2, true);
			
			var manager = new ClientUserManager();
			manager.Join (user);
			
			Assert.AreEqual (user, manager[user.UserId]);
		}

		[Test]
		public void ToggleMuteNullUser ()
		{
			var manager = new ClientUserManager();

			Assert.Throws<ArgumentNullException> (() => manager.ToggleMute (null));
		}
	}
}