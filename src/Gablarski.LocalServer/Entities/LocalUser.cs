using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gablarski.LocalServer.Entities
{
	public class LocalUser
	{
		public virtual int Id
		{
			get; private set;
		}

		public virtual string UserName
		{
			get; set;
		}

		public virtual string EncryptedPassword
		{
			get; set;
		}
	}
}