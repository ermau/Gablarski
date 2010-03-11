using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gablarski.LocalServer.Entities
{
	public class LocalUser
		: User
	{
		public virtual string EncryptedPassword
		{
			get; set;
		}
	}
}