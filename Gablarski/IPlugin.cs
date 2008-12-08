using System;

namespace Gablarski
{
	public interface IPlugin
	{
		string Name { get; }
		string Description { get; }
		Version Version { get; }
	}
}