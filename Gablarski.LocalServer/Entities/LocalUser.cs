using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gablarski.LocalServer.Entities
{
	public class LocalUser
		: UserInfo
	{
		public string UserName
		{
			get; set;
		}

		public string EncryptedPassword
		{
			get; set;
		}
	}
}