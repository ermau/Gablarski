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

namespace Gablarski
{
	public enum UserRegistrationMode
		: byte
	{
		None = 0,
		Normal = 1,
		Approved = 2,
		PreApproved = 3,
		WebPage = 4,
		Message = 5
	}
}

namespace Gablarski.Server
{
	public interface IUserProvider
	{
		/// <summary>
		/// Raised when bans change both deliberately (<see cref="AddBan"/>) or externally.
		/// </summary>
		event EventHandler BansChanged;

		/// <summary>
		/// Gets whether or not users can be updated.
		/// </summary>
		bool UpdateSupported { get; }
		
		/// <summary>
		/// Gets the registration mode of the user provider.
		/// </summary>
		UserRegistrationMode RegistrationMode { get; }

		/// <summary>
		/// Gets the content for any of the various forms of <see cref="UserRegistrationMode"/>.
		/// </summary>
		/// <exception cref="NotSupportedException"><see cref="RegistrationMode"/> is <see cref="UserRegistrationMode.None"/>.</exception>
		/// <remarks>
		/// When <see cref="RegistrationMode"/> is:
		/// <see cref="UserRegistrationMode.Normal"/>: This is the user agreement to register (<c>null</c>/<c>String.Empty</c> to skip.)
		/// <see cref="UserRegistrationMode.WebPage"/>: This is the URL.
		/// <see cref="UserRegistrationMode.Message"/>: This is the message.
		/// </remarks>
		string RegistrationContent { get; }

		/// <summary>
		/// Gets all the users.
		/// </summary>
		IEnumerable<IUser> GetUsers();

		/// <summary>
		/// Gets the usernames for awaiting approvals.
		/// </summary>
		/// <returns>The usernames of awaiting approvals.</returns>
		/// <exception cref="NotSupportedException">Approvals are not supported.</exception>
		IEnumerable<string> GetAwaitingRegistrations();

		/// <summary>
		/// Approves the registration of <paramref name="username"/>.
		/// </summary>
		/// <param name="username">The username to approve.</param>
		/// <exception cref="ArgumentNullException"><paramref name="username"/> is <c>null</c>.</exception>
		/// <exception cref="NotSupportedException">Approvals are not supported.</exception>
		void ApproveRegistration (string username);

		/// <summary>
		/// Rejects the registration of <paramref name="username"/>.
		/// </summary>
		/// <param name="username">The username to reject.</param>
		/// <exception cref="ArgumentNullException"><paramref name="username"/> is <c>null</c>.</exception>
		/// <exception cref="NotSupportedException">Approvals are not supported.</exception>
		void RejectRegistration (string username);

		/// <summary>
		/// Gets whether a user exists or not.
		/// </summary>
		/// <param name="username">The username to check.</param>
		/// <returns><c>true</c> if the username exists, <c>false</c> otherwise</returns>
		/// <exception cref="ArgumentNullException"><paramref name="username"/> is <c>null</c>.</exception>
		bool UserExists (string username);

		/// <summary>
		/// Gets an enumerable of bans persisted.
		/// </summary>
		/// <returns></returns>
		IEnumerable<BanInfo> GetBans();

		/// <summary>
		/// Adds a ban.
		/// </summary>
		/// <param name="ban">The ban to add.</param>
		/// <exception cref="ArgumentNullException"><paramref name="ban"/> is <c>null</c>.</exception>
		void AddBan (BanInfo ban);

		/// <summary>
		/// Removes a ban.
		/// </summary>
		/// <param name="ban">The ban to remove.</param>
		/// <exception cref="ArgumentNullException"><paramref name="ban"/> is <c>null</c>.</exception>
		void RemoveBan (BanInfo ban);

		/// <summary>
		/// Attempts to login a user using the supplied <paramref name="username"/> and <paramref name="password"/>.
		/// </summary>
		/// <param name="username">The username to login with.</param>
		/// <param name="password">The password to login to the username with, <c>null</c> for guests.</param>
		/// <exception cref="ArgumentNullException"><paramref name="username"/></exception>
		LoginResult Login (string username, string password);

		/// <summary>
		/// Registers a new user account with <paramref name="username"/> and <paramref name="password"/>.
		/// </summary>
		/// <param name="username">The username to register.</param>
		/// <param name="password">The password to register <paramref name="username"/> with.</param>
		/// <exception cref="ArgumentNullException"><paramref name="username"/> or <paramref name="password"/> is <c>null</c>.</exception>
		/// <returns>The result of registration.</returns>
		RegisterResult Register (string username, string password);
	}
}