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
using System.Threading;

namespace Gablarski.Server
{
	public class GuestUserProvider
		: IUserProvider
	{
		public event EventHandler BansChanged;

		public bool FirstUserIsAdmin
		{
			get;
			set;
		}

		public bool UpdateSupported
		{
			get { return false; }
		}

		public UserRegistrationMode RegistrationMode
		{
			get { return UserRegistrationMode.None; }
		}

		public string RegistrationContent
		{
			get { throw new NotSupportedException(); }
		}

		public Type IdentifyingType
		{
			get { return typeof (Int32);}
		}

		public IEnumerable<IUser> GetUsers()
		{
			return Enumerable.Empty<IUser>();
		}

		public IEnumerable<string> GetAwaitingRegistrations()
		{
			throw new NotSupportedException();
		}

		public void ApproveRegistration (string username)
		{
			throw new NotSupportedException();
		}

		public void RejectRegistration (string username)
		{
			throw new NotSupportedException();
		}

		public bool UserExists (string username)
		{
			return false;
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

		public LoginResult Login (string username, string password)
		{
			if (username == null)
				throw new ArgumentNullException ("username");

			lock (this.bans)
			{
				if (this.bans.Any (b => !b.IsExpired && b.Username != null && b.Username.Trim().ToLower() == username.Trim().ToLower()))
					return new LoginResult (0, LoginResultState.FailedBanned);
			}

			int next = Interlocked.Decrement (ref this.nextUserId);
			if (FirstUserIsAdmin && next == -1)
				next = 1;
			
			return new LoginResult (next, LoginResultState.Success);
		}

		public RegisterResult Register (string username, string password)
		{
			return RegisterResult.FailedUnsupported;
		}

		private int nextUserId = 0;
		private readonly HashSet<BanInfo> bans = new HashSet<BanInfo>();

		private void OnBansChanged()
		{
			var changed = BansChanged;
			if (changed != null)
				changed (this, EventArgs.Empty);
		}
	}
}