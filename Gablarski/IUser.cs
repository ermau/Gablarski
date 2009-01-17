using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lidgren.Network;
using Gablarski.Server;

namespace Gablarski
{
	/// <summary>
	/// Represents an abstract user provided by an authentication system
	/// </summary>
	public interface IUser
	{
		/// <summary>
		/// Gets the ID of the user
		/// </summary>
		uint ID { get; }

		/// <summary>
		/// Gets the nickname (display name) of the user
		/// </summary>
		string Nickname { get; }

		/// <summary>
		/// Gets the username (registered name) of the user (null if unregistered)
		/// </summary>
		string Username { get; }

		//UserState State { get; }

		/// <summary>
		/// Gets the channel the user is currently in
		/// </summary>
		Channel Channel { get; }
	}

	//[Flags]
	//public enum UserState
	//    : uint
	//{
	//    Unregistered	= 0,
	//    Registered		= 1
	//}
}