using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gablarski.Audio;

namespace Gablarski.Server
{
	public class ServerSourceHandler
		: IServerSourceHandler
	{
		private readonly IServerContext context;
		private readonly IServerSourceManager manager;

		public ServerSourceHandler (IServerContext context, IServerSourceManager manager)
		{
			this.context = context;
			this.manager = manager;
		}

		public IEnumerator<AudioSource> GetEnumerator()
		{
			return Enumerable.Empty<AudioSource>().GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public AudioSource this [int key]
		{
			get { return null; }
		}

		IEnumerable<AudioSource> IServerSourceHandler.this [UserInfo user]
		{
			get { throw new NotImplementedException(); }
		}

		public void Remove (AudioSource source)
		{
			throw new NotImplementedException();
		}
	}

	public interface IServerSourceManager
	{
	}
}