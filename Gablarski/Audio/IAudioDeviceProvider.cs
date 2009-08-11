using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gablarski.Audio
{
	public interface IAudioDeviceProvider
	{
		/// <summary>
		/// Gets a listing of devices for this provider.
		/// </summary>
		/// <returns>A listing of devices to choose from.</returns>
		IEnumerable<IAudioDevice> GetDevices ();

		/// <summary>
		/// Gets the default device for this provider.
		/// </summary>
		IAudioDevice DefaultDevice { get; }
	}
}