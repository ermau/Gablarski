﻿// Copyright (c) 2009, Eric Maupin
// All rights reserved.

// Redistribution and use in source and binary forms, with
// or without modification, are permitted provided that
// the following conditions are met:

// - Redistributions of source code must retain the above 
//   copyright notice, this list of conditions and the
//   following disclaimer.

// - Redistributions in binary form must reproduce the above
//   copyright notice, this list of conditions and the
//   following disclaimer in the documentation and/or other
//   materials provided with the distribution.

// - Neither the name of Gablarski nor the names of its
//   contributors may be used to endorse or promote products
//   derived from this software without specific prior
//   written permission.

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
	public class GuestPermissionProvider
		: IPermissionsProvider
	{
		public event EventHandler<PermissionsChangedEventArgs> PermissionsChanged;

		public bool UpdatedSupported
		{
			get { return false; }
		}

		public IEnumerable<Permission> GetPermissions (int userID)
		{
			if (this.admins.Contains (userID))
				return GetNamesAsPermissions (Permission.GetAllNames());

			if (userID != 0) // User
			{
				return GetNamesAsPermissions (PermissionName.Login, PermissionName.ChangeChannel, PermissionName.AddChannel,
				                              PermissionName.EditChannel, PermissionName.DeleteChannel,
				                              PermissionName.RequestChannelList, PermissionName.RequestSource);
			}
			else // Non-user client.
				return GetNamesAsPermissions (PermissionName.Login, PermissionName.RequestChannelList);
		}


		public void SetAdmin (int userId)
		{
			if (userId == 0)
				throw new ArgumentException ("Guests can not be admins.");

			if (!this.admins.Contains (userId))
				this.admins.Add (userId);

			OnPermissionsChanged (new PermissionsChangedEventArgs (userId));
		}

		private readonly HashSet<int> admins = new HashSet<int> ();

		private IEnumerable<Permission> GetNamesAsPermissions (IEnumerable<PermissionName> names)
		{
			return names.Select (n => new Permission (n, true));
		}

		private IEnumerable<Permission> GetNamesAsPermissions (params PermissionName[] names)
		{
			for (int i = 0; i < names.Length; ++i)
				yield return new Permission (names[i], true);
		}

		protected void OnPermissionsChanged (PermissionsChangedEventArgs e)
		{
			var changed = this.PermissionsChanged;
			if (changed != null)
				changed (this, e);
		}
	}
}