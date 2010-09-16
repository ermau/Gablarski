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

namespace Gablarski.Client
{
	public interface ICurrentUserHandler
		: IUserInfo
	{
		event EventHandler PermissionsChanged;
		event EventHandler<ReceivedJoinResultEventArgs> ReceivedJoinResult;
		event EventHandler<ReceivedLoginResultEventArgs> ReceivedLoginResult;
		event EventHandler<ReceivedRegisterResultEventArgs> ReceivedRegisterResult;
		event EventHandler Kicked;

		IEnumerable<Permission> Permissions { get; }

		/// <summary>
		/// Logs into the connected server
		/// </summary>
		/// <param name="username">The username to log in with.</param>
		/// <param name="password">The password to log in with.</param>
		/// <exception cref="System.ArgumentNullException"><paramref name="username"/> is null or empty.</exception>
		/// <exception cref="System.ArgumentNullException"><paramref name="password"/> is null.</exception>
		void Login (string username, string password);
		
		/// <summary>
		/// Joins the connected server with the specified nickname and password.
		/// </summary>
		/// <param name="nickname">The nickname to use when in the server.</param>
		/// <param name="serverPassword">The password to join the server.</param>
		void Join (string nickname, string serverPassword);

		/// <summary>
		/// Joins the connected server with the specified nickname and password.
		/// </summary>
		/// <param name="nickname">The nickname to use when in the server.</param>
		/// <param name="phonetic">The phonetic spelling for the nickname.</param>
		/// <param name="serverPassword">The password to join the server.</param>
		void Join (string nickname, string phonetic, string serverPassword);

		/// <summary>
		/// Sends a registration to the server.
		/// </summary>
		/// <param name="username">The username to register with.</param>
		/// <param name="password">The password to register with.</param>
		/// <exception cref="ArgumentNullException"><paramref name="username"/> or <paramref name="password"/> is null.</exception>
		/// <exception cref="ArgumentException"><paramref name="username"/> or <paramref name="password"/> is empty.</exception>
		void Register (string username, string password);

		void MuteCapture();
		void UnmuteCapture();

		void MutePlayback();
		void UnmutePlayback();

		void SetComment (string comment);
		void SetStatus (UserStatus status);
	}

	public class ReceivedJoinResultEventArgs
		: EventArgs
	{
		public ReceivedJoinResultEventArgs (LoginResultState result)
		{
			this.Result = result;
		}

		public LoginResultState Result
		{
			get; set;
		}
	}

	public class ReceivedLoginResultEventArgs
		: EventArgs
	{
		public ReceivedLoginResultEventArgs (LoginResult result)
		{
			this.Result = result;
		}

		public LoginResult Result
		{
			get;
			private set;
		}
	}

	public class ReceivedRegisterResultEventArgs
		: EventArgs
	{
		public ReceivedRegisterResultEventArgs (RegisterResult result)
		{
			Result = result;
		}

		public RegisterResult Result
		{
			get;
			private set;
		}
	}
}