using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gablarski
{
	public interface IUserManager
		: IIndexedEnumerable<int, UserInfo>
	{
		void ToggleMute (UserInfo user);
		void Clear ();
	}
}