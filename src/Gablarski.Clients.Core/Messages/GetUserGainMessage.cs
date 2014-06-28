using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gablarski.Clients.Messages
{
	public class GetUserGainMessage
		: UserMessage
	{
		public GetUserGainMessage (IUserInfo user)
			: base (user)
		{
			this.Gain = 1;
		}

		public double Gain
		{
			get;
			set;
		}
	}
}