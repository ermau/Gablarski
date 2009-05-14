using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gablarski.Client
{
	public interface IDeviceProvider
	{
		IEnumerable<IDevice> GetDevices ();
		IDevice DefaultDevice { get; }
	}
}
