using System;

namespace Gablarski.Audio
{
	public interface IAudioDevice
		: IDisposable
	{
		/// <summary>
		/// Gets the unique name identifying this device.
		/// </summary>
		string Name { get; }
	}
}