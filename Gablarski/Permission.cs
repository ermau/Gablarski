using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gablarski
{
	public enum PermissionName
	{
		Login = 1,
		KickPlayerFromChannel = 2,
		KickPlayerFromServer = 9,

		#region Channels
		/// <summary>
		/// Move yourself from channel to channel
		/// </summary>
		ChangeChannel = 7,
		
		/// <summary>
		/// Move a different player from channel to channel
		/// </summary>
		ChangePlayersChannel = 8,
		#endregion

		RequestSource = 3,
		SendAudioToCurrentChannel = 4,
		SendAudioToAll = 5,
		SendAudioToDifferentChannel = 6,

		RequestChannelList = 10,
	}

	public class Permission
	{
		public Permission (PermissionName name)
		{
			this.Name = name;
		}

		public Permission (PermissionName name, bool isAllowed)
			: this (name)
		{
			this.IsAllowed = isAllowed;
		}

		public virtual PermissionName Name
		{
			get;
			private set;
		}

		public virtual bool IsAllowed
		{
			get;
			set;
		}

		public static IEnumerable<PermissionName> GetAllNames ()
		{
			return Enum.GetValues (typeof (PermissionName)).Cast<PermissionName> ();
		}
	}

	public static class PermissionExtensions
	{
		public static bool GetPermission (this IEnumerable<Permission> self, PermissionName name)
		{
			var perm = self.Where (p => p.Name == name).FirstOrDefault ();
			return (perm != null && perm.IsAllowed);
		}
	}
}