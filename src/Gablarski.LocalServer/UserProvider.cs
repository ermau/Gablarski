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
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using Gablarski.Server;
using NHibernate;
using NHibernate.Linq;

namespace Gablarski.LocalServer
{
	public class UserProvider
		: IUserProvider
	{
		public event EventHandler BansChanged;

		public bool UpdateSupported
		{
			get { return true; }
		}

		public bool AllowGuests
		{
			get { return Settings.AllowGuests; }
		}

		public UserRegistrationMode RegistrationMode
		{
			get { return Settings.Registration; }
		}

		public string RegistrationContent
		{
			get { return Settings.RegistrationContent; }
		}

		public IEnumerable<IUser> GetUsers()
		{
			using (var session = Persistance.SessionFactory.OpenSession())
				return session.Query<LocalUser>().Cast<IUser>().ToList();
		}

		public IEnumerable<string> GetAwaitingRegistrations()
		{
			if (RegistrationMode != UserRegistrationMode.Approved)
				throw new NotSupportedException();

			using (var session = Persistance.SessionFactory.OpenSession())
				return session.Query<AwaitingRegistration>().Select (ar => ar.Username).ToList();
		}

		public void ApproveRegistration (string username)
		{
			if (username == null)
				throw new ArgumentNullException ("username");
			if (RegistrationMode != UserRegistrationMode.Approved)
				throw new NotSupportedException();
			
			using (var session = Persistance.SessionFactory.OpenSession())
			using (var transaction = session.BeginTransaction())
			{
				var ar = session.Query<AwaitingRegistration>().SingleOrDefault (r => r.Username == username);
				if (ar == null)
					return;

				CreateUser (session, ar.Username, ar.HashedPassword);
				session.Delete (ar);

				transaction.Commit();
			}
		}

		public void RejectRegistration (string username)
		{
			if (username == null)
				throw new ArgumentNullException ("username");
			if (RegistrationMode != UserRegistrationMode.Approved)
				throw new NotSupportedException();

			username = username.Trim().ToLower();

			using (var session = Persistance.SessionFactory.OpenSession())
			using (var transaction = session.BeginTransaction())
			{
				var ar = session.Query<AwaitingRegistration>().SingleOrDefault (r => r.Username.Trim().ToLower() == username);
				if (ar == null)
					return;

				session.Delete (ar);
				transaction.Commit();
			}
		}

		public bool UserExists (string username)
		{
			if (username == null)
				throw new ArgumentNullException ("username");

			using (var session = Persistance.SessionFactory.OpenSession())
				return UserExists (session, username);
		}

		public IEnumerable<BanInfo> GetBans()
		{
			using (var session = Persistance.SessionFactory.OpenSession())
				return session.Query<LocalBanInfo>().Cast<BanInfo>().ToList();
		}

		public void AddBan (BanInfo ban)
		{
			if (ban == null)
				throw new ArgumentNullException ("ban");

			using (var session = Persistance.SessionFactory.OpenSession())
			{
				session.SaveOrUpdate (new LocalBanInfo
				{
					Created = DateTime.Now,
					IPMask = ban.IPMask,
					Length = ban.Length,
					Username = ban.Username
				});
			}
		}

		public void RemoveBan (BanInfo ban)
		{
			if (ban == null)
				throw new ArgumentNullException ("ban");

			using (var session = Persistance.SessionFactory.OpenSession())
			{
				LocalBanInfo localBan = (ban as LocalBanInfo)
										?? session.Query<LocalBanInfo>().FirstOrDefault (b => b.Username == ban.Username || b.IPMask == ban.IPMask);

				if (localBan == null)
					return;

				session.Delete (localBan);
			}
		}

		public LoginResult Login (string username, string password)
		{
			if (username == null)
				throw new ArgumentNullException ("username");

			using (var session = Persistance.SessionFactory.OpenSession())
			{
				if (session.Query<LocalBanInfo>().Any (b => b.Username == username))
					return new LoginResult (0, LoginResultState.FailedBanned);

				var user = session.Query<LocalUser>().FirstOrDefault (u => u.Username == username);
				if (user == null)
				{
					if (password == null && AllowGuests)
						return new LoginResult (Interlocked.Decrement (ref guestId), LoginResultState.Success);
					else
						return new LoginResult (0, LoginResultState.FailedUsername);
				}

				if (password == null || user.HashedPassword !=  HashPassword (password))
					return new LoginResult (0, LoginResultState.FailedPassword);

				return new LoginResult (user.UserId, LoginResultState.Success);
			}
		}

		public RegisterResult Register (string username, string password)
		{
			if (username == null)
				throw new ArgumentNullException ("username");
			if (password == null)
				throw new ArgumentNullException ("password");

			using (var session = Persistance.SessionFactory.OpenSession())
			using (var trans = session.BeginTransaction())
			{
				if (RegistrationMode == UserRegistrationMode.Normal || RegistrationMode == UserRegistrationMode.PreApproved)
				{
					if (UserExists (session, username))
						return RegisterResult.FailedUsernameInUse;

					CreateUser (session, username, HashPassword (password));
					
					trans.Commit();
					return RegisterResult.Success;
				}
				else if (RegistrationMode == UserRegistrationMode.Approved)
				{
					session.SaveOrUpdate (new AwaitingRegistration
					{
						Username = username,
						HashedPassword = HashPassword (password)
					});

					trans.Commit();
					return RegisterResult.Success;
				}
				else
				{
					trans.Rollback();
					return RegisterResult.FailedUnsupported;
				}
			}
		}

		private bool UserExists (ISession session, string username)
		{
			return session.Query<LocalUser>().Any (u => u.Username == username);
		}

		private int guestId;
		private readonly SHA1CryptoServiceProvider sha = new SHA1CryptoServiceProvider();
		private string HashPassword (string password)
		{
			return sha.ComputeHash (Encoding.ASCII.GetBytes (password)).Select (b => b.ToString ("X2")).Aggregate ((s1, s2) => s1 + s2);
		}

		private void CreateUser (ISession session, string username, string hashedPassword)
		{
			bool first = !session.Query<LocalUser>().Any();

			var user = new LocalUser { HashedPassword = hashedPassword, Username = username };
			session.SaveOrUpdate (user);

			if (!first)
			{
				PermissionProvider.CreatePermissions (session, user.UserId,
					PermissionName.SendAudio,
					PermissionName.RequestUserList,
					PermissionName.RequestSource,
					PermissionName.RequestChannelList,
					PermissionName.ChangeChannel);
			}
			else
			{
				PermissionProvider.CreatePermissions (session, user.UserId, (PermissionName[])Enum.GetValues (typeof(PermissionName)));
			}
		}
	}
}