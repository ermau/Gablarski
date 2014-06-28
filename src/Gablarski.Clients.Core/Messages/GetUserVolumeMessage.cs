using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gablarski.Clients.Messages
{
	public class GetUserVolumeMessage
		: UserMessage
	{
		public GetUserVolumeMessage (IUserInfo user)
			: base (user)
		{
			Volume = 1;
		}

		public double Volume
		{
			get;
			set;
		}
	}
}