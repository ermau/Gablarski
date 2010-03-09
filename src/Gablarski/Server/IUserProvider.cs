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

namespace Gablarski.Server
{
	public enum RegistrationMode
	{
		Normal = 0,
		WebPage = 1,
		Message = 2
	}

	public interface IUserProvider
	{
		/// <summary>
		/// Gets whether or not users can be updated.
		/// </summary>
		bool UpdateSupported { get; }

		/// <summary>
		/// Gets the registration mode of the user provider.
		/// </summary>
		/// <exception cref="NotSupportedException"><see cref="UpdateSupported"/> is <c>false</c>.</exception>
		RegistrationMode RegistrationMode { get; }

		/// <summary>
		/// Gets the content for any of the various forms of <see cref="Server.RegistrationMode"/>.
		/// </summary>
		/// <exception cref="NotSupportedException"><see cref="UpdateSupported"/> is <c>false</c>.</exception>
		/// <remarks>
		/// When <see cref="RegistrationMode"/> is:
		/// <see cref="Server.RegistrationMode.Normal"/>: This is the user agreement to register (<c>null</c>/<c>String.Empty</c> to skip.)
		/// <see cref="Server.RegistrationMode.WebPage"/>: This is the URL.
		/// <see cref="Server.RegistrationMode.Message"/>: This is the message.
		/// </remarks>
		string RegistrationContent { get; }

		/// <summary>
		/// Gets all the users.
		/// </summary>
		IEnumerable<User> GetUsers();

		/// <summary>
		/// Gets whether a user exists or not.
		/// </summary>
		/// <param name="username">The username to check.</param>
		/// <returns><c>true</c> if the username exists, <c>false</c> otherwise</returns>
		/// <exception cref="ArgumentNullException"><paramref name="username"/> is <c>null</c>.</exception>
		bool UserExists (string username);

		/// <summary>
		/// Attempts to login a user using the supplied <paramref name="username"/> and <paramref name="password"/>.
		/// </summary>
		/// <param name="username">The username to login with.</param>
		/// <param name="password">The password to login to the username with.</param>
		/// <exception cref="ArgumentNullException"><paramref name="username"/> or <paramref name="password"/> is <c>null</c>.</exception>
		LoginResult Login (string username, string password);

		/// <summary>
		/// Registers a new user account with <paramref name="username"/> and <paramref name="password"/>.
		/// </summary>
		/// <param name="username">The username to register.</param>
		/// <param name="password">The password to register <paramref name="username"/> with.</param>
		/// <exception cref="ArgumentNullException"><paramref name="username"/> or <paramref name="password"/> is <c>null</c>.</exception>
		LoginResult Register (string username, string password);
	}
}