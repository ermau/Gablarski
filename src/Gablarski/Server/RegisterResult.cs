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

using System.Linq;

namespace Gablarski
{
	public enum RegisterResult
		: byte
	{
		/// <summary>
		/// Failed for an unknown reason.
		/// </summary>
		FailedUnknown = 0,

		/// <summary>
		/// Succeeded (or entered successfully for approval.)
		/// </summary>
		Success = 1,

		/// <summary>
		/// Failed because the username is invalid.
		/// </summary>
		FailedUsername = 2,

		/// <summary>
		/// Failed because the password is invalid.
		/// </summary>
		FailedPassword = 3,

		/// <summary>
		/// Failed because the username is already in use.
		/// </summary>
		FailedUsernameInUse = 4,

		/// <summary>
		/// Failed because the user provider does not support registration or it has been disabled.
		/// </summary>
		FailedUnsupported = 5,

		/// <summary>
		/// Failed because in this pre-approval system of registration, you are not pre-approved.
		/// </summary>
		FailedNotApproved = 6,
	}
}