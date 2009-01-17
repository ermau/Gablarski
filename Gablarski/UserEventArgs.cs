using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gablarski
{
	public class SourceEventArgs
		: UserEventArgs
	{
		public SourceEventArgs (IUser user, IMediaSource source)
			: base (user)
		{
			this.Source = source;
		}

		public IMediaSource Source
		{
			get;
			private set;
		}
	}

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

	public class UserListEventArgs
		: EventArgs
	{
		public UserListEventArgs (IEnumerable<IUser> users)
		{
			this.Users = users;
		}

		public IEnumerable<IUser> Users
		{
			get; private set;
		}
	}

	public class AudioEventArgs
		: MediaEventArgs
	{
		public AudioEventArgs (IMediaSource source, byte[] data)
			: base(source)
		{
			this.Data = data;
		}

		public byte[] Data
		{
			get;
			private set;
		}
	}

	public class MediaEventArgs
		: EventArgs
	{
		public MediaEventArgs (IMediaSource source)
		{
			this.Source = source;
		}

		public IMediaSource Source
		{
			get; private set;
		}
	}
}