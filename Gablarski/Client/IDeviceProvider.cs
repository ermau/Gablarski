using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gablarski.Client
{
	public interface IDeviceProvider
	{
		/// <summary>
		/// Gets a listing of devices for this provider.
		/// </summary>
		/// <returns>A listing of devices to choose from.</returns>
		IEnumerable<IDevice> GetDevices ();

		/// <summary>
		/// Gets the default device for this provider.
		/// </summary>
		IDevice DefaultDevice { get; }
	}
}
