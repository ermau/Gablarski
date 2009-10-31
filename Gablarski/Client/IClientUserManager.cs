using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gablarski.Client
{
	public interface IClientUserManager
		: IUserManager
	{
		bool GetIsIgnored (UserInfo user);
		bool ToggleIgnore (UserInfo user);
	}
}