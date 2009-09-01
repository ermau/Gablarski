using System;
using System.Threading;

namespace Gablarski.Server
{
	public class GuestAuthProvider
		: IAuthenticationProvider
	{
		#region IAuthenticationProvider Members
		public Type IdentifyingType
		{
			get { return typeof (Int32);}
		}

		public bool UserExists (string username)
		{
			return false;
		}

		public LoginResult Login (string username, string password)
		{
			return new LoginResult (Interlocked.Increment (ref this.nextUserId), LoginResultState.Success);
		}

		#endregion

		private int nextUserId = 0;
	}
}