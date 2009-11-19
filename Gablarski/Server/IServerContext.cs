using System;

namespace Gablarski.Server
{
	public interface IServerContext
	{
		IServerUserManager Users { get; }
	}
}
