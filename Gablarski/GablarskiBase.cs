using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Gablarski
{
	public abstract class GablarskiBase
	{
		public event EventHandler<UserEventArgs> UserLogin;
		public event EventHandler<UserEventArgs> UserLogout;
		public event EventHandler<MediaEventArgs> AudioReceived;

		public event EventHandler<SourceEventArgs> SourceCreated;
		public event EventHandler<SourceEventArgs> SourceDestroyed;

		protected virtual void OnSourceCreated (SourceEventArgs e)
		{
			var created = this.SourceCreated;
			if (created != null)
				created(this, e);
		}

		protected virtual void OnUserLogin (UserEventArgs e)
		{
			var login = this.UserLogin;
			if (login != null)
				login (this, e);
		}

		protected virtual void OnUserLogout (UserEventArgs e)
		{
			var ulogout = this.UserLogout;
			if (ulogout != null)
				ulogout(this, e);
		}

		protected virtual void OnAudioReceived (MediaEventArgs e)
		{
			var audio = this.AudioReceived;
			if (audio != null)
				audio(this, e);
		}
	}
}