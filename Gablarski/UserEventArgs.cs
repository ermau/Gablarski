using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gablarski
{
	public class UserEventArgs
		: EventArgs
	{
		public UserEventArgs (IUser user)
		{
			this.User = user;
		}

		public IUser User
		{
			get;
			private set;
		}
	}

	public class VoiceEventArgs
		: UserEventArgs
	{
		public VoiceEventArgs (IUser user, byte[] voiceData)
			: base (user)
		{
			this.VoiceData = voiceData;
		}

		public byte[] VoiceData
		{
			get;
			private set;
		}
	}
}