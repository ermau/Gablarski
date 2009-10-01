using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Gablarski.Audio;

namespace Gablarski
{
	public partial class DeviceSelector
		: UserControl
	{
		public DeviceSelector ()
		{
			InitializeComponent ();

			this.provider.DisplayMember = "Name";
		}

		public IEnumerable<Type> ProviderSource
		{
			set
			{
				this.provider.DataSource = value.ToList();
			}
		}

		public Type Provider
		{
			get { return (provider.SelectedItem != null) ? (Type)provider.SelectedItem : null; }
		}

		public IAudioDevice Device
		{
			get { return (device.SelectedItem != null) ? (IAudioDevice)device.SelectedItem : null; }
		}

		public void SetProvider (string providerName)
		{
			if (String.IsNullOrEmpty (providerName))
				return;

			provider.SelectedItem = provider.Items.Cast<Type>().FirstOrDefault (d => d.AssemblyQualifiedName == providerName);
		}

		public void SetDevice (string deviceName)
		{
			if (String.IsNullOrEmpty (deviceName))
				return;

			device.SelectedItem = device.Items.Cast<IAudioDevice>().FirstOrDefault (d => d.Name == deviceName);
		}

		private void provider_SelectedIndexChanged (object sender, EventArgs e)
		{
			this.device.Items.Clear();

			if (this.provider.SelectedItem == null)
				this.device.Enabled = false;
			else
			{
				this.device.Enabled = true;
				using (var p = ((IAudioDeviceProvider)Activator.CreateInstance (Provider)))
				{
					this.device.DataSource = p.GetDevices().ToList();
					this.device.SelectedItem = p.DefaultDevice;
				}
			}
		}
	}
}
