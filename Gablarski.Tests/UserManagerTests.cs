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
using NUnit.Framework;

namespace Gablarski.Tests
{
	[TestFixture]
	public class UserManagerTests
	{
		[Test]
		public void Join ()
		{
			var user = new UserInfo ("Nickname", "Username", 1, 2, true);

			var manager = new UserManager ();
			manager.Join (user);

			Assert.IsTrue (manager.IsJoined (user));
			Assert.IsTrue (((IEnumerable<UserInfo>)manager).Contains (user));
			Assert.AreEqual (user, manager[user.UserId]);
		}
		
		[Test]
		public void JoinNullUser ()
		{
			var manager = new UserManager ();
			Assert.Throws<ArgumentNullException> (() => manager.Join (null));
		}
		
		[Test]
		public void Depart()
		{
			var user = new UserInfo ("Nickname", "Username", 1, 2, true);
			
			var manager = new UserManager();
			manager.Join (user);
			
			Assert.IsTrue (manager.IsJoined (user));
			Assert.IsTrue (((IEnumerable<UserInfo>)manager).Contains (user));
			Assert.AreEqual (user, manager[user.UserId]);
			
			Assert.IsTrue (manager.Depart (user));
			
			Assert.IsFalse (manager.IsJoined (user));
			Assert.IsFalse (((IEnumerable<UserInfo>)manager).Contains (user));
			Assert.AreEqual (null, manager[user.UserId]);
		}
		
		[Test]
		public void DepartNonJoined()
		{
			var user = new UserInfo ("Nickname", "Username", 1, 2, true);
			
			var manager = new UserManager();
			
			Assert.IsFalse (manager.IsJoined (user));
			Assert.IsFalse (manager.Depart (user));
		}
		
		[Test]
		public void DepartNullUser()
		{
			var manager = new UserManager();
			Assert.Throws<ArgumentNullException> (() => manager.Depart (null));
		}
		
		[Test]
		public void UpdateNullUsers()
		{
			var manager = new UserManager();
			Assert.Throws<ArgumentNullException> (() => manager.Update ((IEnumerable<UserInfo>)null));
		}
		
		[Test]
		public void UpdateEmpty()
		{
			var user = new UserInfo ("Nickname", "Username", 1, 2, true);
			var user2 = new UserInfo ("Nickname2", "Username2", 2, 3, false);
			
			var manager = new UserManager();
			
			manager.Update (new [] { user, user2 });
			
			Assert.IsTrue (manager.IsJoined (user));
			Assert.IsTrue (manager.IsJoined (user2));
		}
		
		[Test]
		public void Update()
		{
			var user = new UserInfo ("Nickname", "Username", 1, 2, true);
			
			var manager = new UserManager();
			manager.Join (user);
			
			user = new UserInfo ("NicknameU", "Username2", 1, 2, false);
			var user2 = new UserInfo ("Nickname2U", "Username2U", 2, 3, false);
			
			manager.Update (new [] { user, user2 });
			
			Assert.IsTrue (manager.IsJoined (user));
			Assert.AreEqual (user, manager[user.UserId]);
			Assert.AreEqual (user.Nickname, manager[user.UserId].Nickname);
			Assert.AreEqual (user.Username, manager[user.UserId].Username);
			Assert.AreEqual (user.UserId, manager[user.UserId].UserId);
			Assert.AreEqual (user.CurrentChannelId, manager[user.UserId].CurrentChannelId);
			Assert.AreEqual (user.IsMuted, manager[user.UserId].IsMuted);
			
			Assert.IsTrue (manager.IsJoined (user2));
			Assert.AreEqual (user2, manager[user2.UserId]);
			Assert.AreEqual (user2.Nickname, manager[user2.UserId].Nickname);
			Assert.AreEqual (user2.Username, manager[user2.UserId].Username);
			Assert.AreEqual (user2.UserId, manager[user2.UserId].UserId);
			Assert.AreEqual (user2.CurrentChannelId, manager[user2.UserId].CurrentChannelId);
			Assert.AreEqual (user2.IsMuted, manager[user2.UserId].IsMuted);
		}
		
		[Test]
		public void UpdateNullUser()
		{
			var manager = new UserManager();
			Assert.Throws<ArgumentNullException> (() => manager.Update ((UserInfo)null));
		}
		
		[Test]
		public void UpdateUser()
		{
			var user = new UserInfo ("Nickname", "Username", 1, 2, true);
			var user2 = new UserInfo ("Nickname2", "Username2", 2, 3, false);
			
			var manager = new UserManager();
			manager.Join (user);
			
			user = new UserInfo ("NicknameU", "Username2", 1, 2, false);
			user2 = new UserInfo ("Nickname2U", "Username2U", 2, 3, false);
			
			manager.Update (user);
			
			Assert.IsTrue (manager.IsJoined (user));
			Assert.AreEqual (user, manager[user.UserId]);
			Assert.AreEqual (user.Nickname, manager[user.UserId].Nickname);
			Assert.AreEqual (user.Username, manager[user.UserId].Username);
			Assert.AreEqual (user.UserId, manager[user.UserId].UserId);
			Assert.AreEqual (user.CurrentChannelId, manager[user.UserId].CurrentChannelId);
			Assert.AreEqual (user.IsMuted, manager[user.UserId].IsMuted);
			
			manager.Update (user2);
			
			Assert.IsTrue (manager.IsJoined (user2));
			Assert.AreEqual (user2, manager[user2.UserId]);
			Assert.AreEqual (user2.Nickname, manager[user2.UserId].Nickname);
			Assert.AreEqual (user2.Username, manager[user2.UserId].Username);
			Assert.AreEqual (user2.UserId, manager[user2.UserId].UserId);
			Assert.AreEqual (user2.CurrentChannelId, manager[user2.UserId].CurrentChannelId);
			Assert.AreEqual (user2.IsMuted, manager[user2.UserId].IsMuted);
		}

		[Test]
		public void IsJoined()
		{
			var user = new UserInfo ("Nickname", "Username", 1, 2, true);
			var user2 = new UserInfo ("Nickname2", "Username2", 2, 3, false);
			
			var manager = new UserManager();
			manager.Join (user);
			
			Assert.IsTrue (manager.IsJoined (user));
			Assert.IsFalse (manager.IsJoined (user2));
			
			manager.Join (user2);
			Assert.IsTrue (manager.IsJoined (user2));
			
			manager.Depart (user);
			Assert.IsFalse (manager.IsJoined (user));
		}
		
		[Test]
		public void IsJoinedId()
		{
			var user = new UserInfo ("Nickname", "Username", 1, 2, true);
			var user2 = new UserInfo ("Nickname2", "Username2", 2, 3, false);
			
			var manager = new UserManager();
			manager.Join (user);
			
			Assert.IsTrue (manager.IsJoined (user.UserId));
			Assert.IsFalse (manager.IsJoined (user2.UserId));
			
			manager.Join (user2);
			Assert.IsTrue (manager.IsJoined (user2.UserId));
			
			manager.Depart (user);
			Assert.IsFalse (manager.IsJoined (user.UserId));
		}
		
		[Test]
		public void IsJoinedNullUser()
		{
			var manager = new UserManager();
			Assert.Throws<ArgumentNullException> (() => manager.IsJoined (null));
		}
		
		[Test]
		public void TryGetValueFound()
		{
			var user = new UserInfo ("Nickname", "Username", 1, 2, true);
			var user2 = new UserInfo ("Nickname2", "Username2", 2, 3, false);
			
			var manager = new UserManager();
			manager.Join (user);
			
			UserInfo foundUser;
			Assert.IsTrue (manager.TryGetUser (user.UserId, out foundUser));
			Assert.AreEqual (user, foundUser);
		}
		
		[Test]
		public void TryGetValueNotFound()
		{
			var user = new UserInfo ("Nickname", "Username", 1, 2, true);
			var user2 = new UserInfo ("Nickname2", "Username2", 2, 3, false);
			
			var manager = new UserManager();
			manager.Join (user2);
			
			UserInfo foundUser;
			Assert.IsFalse (manager.TryGetUser (user.UserId, out foundUser));
			Assert.AreEqual (null, foundUser);
		}
		
		[Test]
		public void IndexerNotPresent()
		{
			var user = new UserInfo ("Nickname", "Username", 1, 2, true);
			var user2 = new UserInfo ("Nickname2", "Username2", 2, 3, false);
			
			var manager = new UserManager();
			manager.Join (user);
			
			Assert.AreEqual (null, manager[user2.UserId]);
		}
		
		[Test]
		public void IndexerPresent()
		{
			var user = new UserInfo ("Nickname", "Username", 1, 2, true);
			
			var manager = new UserManager();
			manager.Join (user);
			
			Assert.AreEqual (user, manager[user.UserId]);
		}

		[Test]
		public void ToggleMuteNullUser ()
		{
			var manager = new UserManager();

			Assert.Throws<ArgumentNullException> (() => manager.ToggleMute (null));
		}
	}
}