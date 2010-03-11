using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gablarski.LocalServer.Entities
{
	public class LocalUser
		: IUser
	{
		public virtual int UserId
		{
			get;
			private set;
		}

		public virtual string Username
		{
			get;
			set;
		}

		public virtual string EncryptedPassword
		{
			get; set;
		}
	}
}