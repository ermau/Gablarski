using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lidgren.Network;

namespace Gablarski
{
	internal class DecodedUser
		: IUser
	{
		#region IUser Members

		public int ID
		{
			get; set;
		}

		public string Nickname
		{
			get; set;
		}

		public string Username
		{
			get; set;
		}

		//public UserState State
		//{
		//    get; set;
		//}

		public Channel Channel
		{
			get; set;
		}

		#endregion
	}
}