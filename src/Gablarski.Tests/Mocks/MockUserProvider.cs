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
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using Gablarski;
using Gablarski.Server;

namespace Gablarski.Tests
{
	public class MockUser
		: IUser
	{
		public MockUser (int userId, string username, string password)
		{
			UserId = userId;
			Username = username;
			Password = password;
		}
		
		public int UserId
		{
			get; private set;
		}
		
		public string Username
		{
			get; private set;
		}
		
		public string Password
		{
			get; private set;
		}
	}

	public class MockUserProvider
		: IUserProvider
	{
		public event EventHandler BansChanged;
	
		public bool UpdateSupported
		{
			get; set;
		}
		
		public UserRegistrationMode RegistrationMode
		{
			get; set;
		}
		
		public string RegistrationContent
		{
			get; set;
		}

		public void AddUser (MockUser user)
		{
			users.Add (user);
		}

		public IEnumerable<string> GetAwaitingRegistrations()
		{
			return this.awaitingApproval.Select (mu => mu.Username);
		}

		public void ApproveRegistration (string username)
		{
			if (username == null)
				throw new ArgumentNullException ("username");
			if (RegistrationMode != UserRegistrationMode.Approved && RegistrationMode != UserRegistrationMode.PreApproved)
				throw new NotSupportedException();

			MockUser user = this.awaitingApproval.FirstOrDefault (u => u.Username == username);
			if (user == null)
				return;

			this.awaitingApproval.Remove (user);
			this.users.Add (user);
		}

		public void RejectRegistration (string username)
		{
			if (username == null)
				throw new ArgumentNullException ("username");
			if (RegistrationMode != UserRegistrationMode.Approved && RegistrationMode != UserRegistrationMode.PreApproved)
				throw new NotSupportedException();

			this.awaitingApproval.RemoveAll (m => m.Username == username);
		}

		public bool UserExists (string username)
		{
			if (username == null)
				throw new ArgumentNullException ("username");
			
			username = username.Trim().ToLower();
			
			return users.Concat (awaitingApproval).Any (u => u.Username.Trim().ToLower() == username);
		}

		public LoginResult Login (string username, string password)
		{
			if (username == null)
				throw new ArgumentNullException ("username");
			
			username = username.Trim().ToLower();

			if (this.bans.Any (b => !b.IsExpired && b.Username != null && b.Username.Trim().ToLower() == username))
				return new LoginResult (0, LoginResultState.FailedBanned);
			
			LoginResultState state = LoginResultState.Success;
			MockUser user = users.FirstOrDefault (u => u.Username.Trim().ToLower() == username);
			if (user != null)
			{
				if (password == null)
					state = LoginResultState.FailedPassword;
				else if (password.Trim().ToLower() != user.Password.Trim().ToLower())
					state = LoginResultState.FailedPassword;
			}
			else
			{
				if (password != null)
					state = LoginResultState.FailedUsernameAndPassword;
			}
			
			return new LoginResult ((user != null) ? user.UserId : Interlocked.Decrement (ref nextGuestId), state);
		}
		
		public RegisterResult Register (string username, string password)
		{
			if (RegistrationMode != UserRegistrationMode.Normal && RegistrationMode != UserRegistrationMode.Approved && RegistrationMode != UserRegistrationMode.PreApproved)
				return RegisterResult.FailedUnsupported;
			if (username == null)
				throw new ArgumentNullException ("username");

			RegisterResult state = RegisterResult.FailedUnknown;
			if (username.Trim() == String.Empty)
				state = RegisterResult.FailedUsername;
			else if (UserExists (username))
				state = RegisterResult.FailedUsernameInUse;
			else
			{
				state = RegisterResult.Approved;
				int userId = ((users.Any()) ? users.Max (u => u.UserId) : 0) + 1;

				if (RegistrationMode != UserRegistrationMode.Approved)
					this.users.Add (new MockUser (userId, username, password));
				else
					this.awaitingApproval.Add (new MockUser (userId, username, password));
			}

			return state;
		}
		
		public IEnumerable<IUser> GetUsers()
		{
			return this.users.Cast<IUser>();
		}

		public IEnumerable<BanInfo> GetBans()
		{
			lock (this.bans)
				return this.bans.ToList();
		}

		public void AddBan (BanInfo ban)
		{
			if (ban == null)
				throw new ArgumentNullException ("ban");
			
			lock (this.bans)
				this.bans.Add (ban);

			OnBansChanged();
		}

		public void RemoveBan (BanInfo ban)
		{
			if (ban == null)
				throw new ArgumentNullException ("ban");

			lock (this.bans)
				this.bans.Remove (ban);

			OnBansChanged();
		}

		private readonly HashSet<BanInfo> bans = new HashSet<BanInfo>();
		private readonly List<MockUser> awaitingApproval = new List<MockUser>();
		private readonly List<MockUser> users = new List<MockUser>();
		private int nextGuestId;

		private void OnBansChanged()
		{
			var changed = BansChanged;
			if (changed != null)
				changed (this, EventArgs.Empty);
		}
	}
}