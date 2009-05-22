using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gablarski
{
	public interface IUserProvider
	{
		/// <summary>
		/// Gets whether a user exists or not.
		/// </summary>
		/// <param name="username">The username to check.</param>
		/// <returns><c>true</c> if the username exists, <c>false</c> otherwise</returns>
		//bool UserExists (string username);

		/// <summary>
		/// Attempts to login a user using the supplied username and password.
		/// </summary>
		/// <param name="username">The username to login with.</param>
		/// <param name="password">The password to login to the username with.</param>
		/// <returns></returns>
		LoginResult Login (string username, string password);
	}

	public enum LoginResultState
		: byte
	{
		/// <summary>
		/// Failed for an unknown reason.
		/// </summary>
		FailedUnknown = 0,

		/// <summary>
		/// Succeeded.
		/// </summary>
		Success = 1,

		/// <summary>
		/// Failed because the username does not exist, is invalid.
		/// </summary>
		FailedUsername = 2,

		/// <summary>
		/// Failed because the password does not match the username.
		/// </summary>
		FailedPassword = 3,

		/// <summary>
		/// Failed because the username and password combination was not found.
		/// </summary>
		/// <remarks>For providers that do not wish to reveal if a username exists.</remarks>
		FailedUsernameAndPassword = 4,

		/// <summary>
		/// Failed because the nickname is already in use.
		/// </summary>
		FailedNicknameInUse = 5,
	}

	/// <summary>
	/// Provides results for an attempted login.
	/// </summary>
	public class LoginResult
	{
		internal LoginResult (IValueReader reader)
		{
			this.Deserialize (reader);
		}

		public LoginResult (long userId, LoginResultState state)
		{
			this.PlayerId = userId;
			this.ResultState = state;
		}

		/// <summary>
		/// Gets the logged-in players ID.
		/// </summary>
		public long PlayerId
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets whether the login succeeded or not.
		/// </summary>
		public bool Succeeded
		{
			get { return (this.ResultState == LoginResultState.Success); }
		}

		/// <summary>
		/// Gets the reason for a login failure, <c>null</c> otherwise.
		/// </summary>
		public LoginResultState ResultState
		{
			get;
			internal set;
		}

		internal void Serialize (IValueWriter writer)
		{
			writer.WriteInt64 (this.PlayerId);
			writer.WriteByte ((byte)this.ResultState);
		}

		internal void Deserialize (IValueReader reader)
		{
			this.PlayerId = reader.ReadInt64();
			this.ResultState = (LoginResultState)reader.ReadByte ();
		}
	}
}