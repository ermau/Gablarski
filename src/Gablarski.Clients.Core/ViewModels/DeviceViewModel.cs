using System;
using Gablarski.Audio;

namespace Gablarski.Clients.ViewModels
{
	public class DeviceViewModel
	{
		private readonly IAudioDeviceProvider provider;
		private readonly IAudioDevice device;

		public DeviceViewModel (IAudioDeviceProvider provider, IAudioDevice device)
		{
			if (provider == null)
				throw new ArgumentNullException ("provider");
			if (device == null)
				throw new ArgumentNullException ("device");

			this.provider = provider;
			this.device = device;
		}

		public string DisplayName
		{
			get
			{
				if (Device is DefaultDevice)
					return String.Format ("Default ({0})", provider.DefaultDevice.Name);
				else
					return this.device.Name;
			}
		}

		public IAudioDevice Device
		{
			get { return this.device; }
		}
	}
}
