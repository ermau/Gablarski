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
using System.Text;

namespace Gablarski.Server
{
	/// <summary>
	/// Contract for providers of permissions
	/// </summary>
	public interface IPermissionsProvider
	{
		/// <summary>
		/// Fired when permissions have changed.
		/// </summary>
		event EventHandler<PermissionsChangedEventArgs> PermissionsChanged;

		/// <summary>
		/// Gets whether or not the permissions provided can be updated.
		/// </summary>
		bool UpdatedSupported { get; }

		/// <remarks>
		/// &lt;0 is guests.
		/// 0 is connections only
		/// &gt;1 is logged in users.
		/// </remarks>
		IEnumerable<Permission> GetPermissions (int userId);

		/// <summary>
		/// Sets permissions for the specified user id.
		/// </summary>
		/// <param name="userId">The user id to set permissions for.</param>
		/// <param name="permissions">The permissions to set for the user.</param>
		/// <exception cref="NotSupportedException">If <see cref="UpdatedSupported"/> is <c>false</c>.</exception>
		/// <exception cref="ArgumentNullException"><paramref name="permissions"/> is <c>null</c>.</exception>
		/// <remarks>
		/// This should only update permissions in <paramref name="permissions"/>, it's not a blanket set.
		/// </remarks>
		void SetPermissions (int userId, IEnumerable<Permission> permissions);
	}

	public class PermissionsChangedEventArgs
		: EventArgs
	{
		public PermissionsChangedEventArgs (int userId)
		{
			this.UserId = userId;
		}

		public int UserId
		{
			get; private set;
		}
	}
}