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
using Cadenza.Collections;
using Gablarski.Server;
using NHibernate;
using NHibernate.Linq;

namespace Gablarski.LocalServer
{
	public class PermissionProvider
		: IPermissionsProvider
	{
		public event EventHandler<PermissionsChangedEventArgs> PermissionsChanged;

		/// <summary>
		/// Gets whether or not the permissions provided can be updated.
		/// </summary>
		public bool UpdatedSupported
		{
			get { return true; }
		}

		public void SetPermissions (int userId, IEnumerable<Gablarski.Permission> permissions)
		{
			lock (permissionCache)
			{
				permissionCache.Remove (userId);

				using (var session = Persistance.SessionFactory.OpenSession())
				using (var trans = session.BeginTransaction())
				{
					foreach (var permission in permissions)
					{
						Permission realp = session.Query<Permission>().FirstOrDefault (p => p.UserID == userId && p.Name == permission.Name);
						if (realp == null)
							CreatePermission (session, permission.Name, userId, permission.ChannelId, permission.IsAllowed);
						else
							realp.Allowed = permission.IsAllowed;

						session.SaveOrUpdate (realp);
					}

					trans.Commit();
				}
			}
		}

		public IEnumerable<Gablarski.Permission> GetPermissions (int userId)
		{
			userId = (userId < 0) ? -1 : userId;

			lock (permissionCache)
			{
				if (!permissionCache.Contains (userId))
				{
					using (var session = Persistance.SessionFactory.OpenSession())
					{
						var permissions = session.Query<Permission>().Where (p => p.UserID == userId);
						foreach (var p in permissions)
							permissionCache.Add (userId, new Gablarski.Permission (p.Name, p.Allowed));
					}
				}

				return permissionCache[userId].ToList();
			}
		}

		public static void Setup (ISession session)
		{
			CreatePermissions (session, -1,
				PermissionName.SendAudio,
				PermissionName.RequestUserList,
				PermissionName.RequestSource,
				PermissionName.RequestChannelList,
				PermissionName.ChangeChannel);
		}

		private readonly MutableLookup<int, Gablarski.Permission> permissionCache = new MutableLookup<int, Gablarski.Permission>();

		internal static void CreatePermissions (ISession session, int userId, params PermissionName[] permissions)
		{
			for (int i = 0; i < permissions.Length; ++i)
				CreatePermission (session, permissions[i], userId, 0, true);
		}

		internal static void CreatePermission (ISession session, PermissionName name, int userId, int channelId)
		{
			CreatePermission (session, name, userId, channelId, true);
		}

		internal static void CreatePermission (ISession session, PermissionName name, int userId, int channelId, bool allowed)
		{
			session.SaveOrUpdate (new Permission
			{
				Allowed = allowed,
				Name = name,
				UserID = userId,
				ChannelId = channelId
			});
		}
	}
}