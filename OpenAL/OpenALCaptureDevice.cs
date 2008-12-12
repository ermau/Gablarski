using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gablarski.Client.Providers.OpenAL
{
	public class OpenALCaptureDevice
		: ICaptureDevice
	{
		internal OpenALCaptureDevice (string deviceName)
		{
			this.deviceName = deviceName;
		}

		#region ICaptureDevice Members

		public object Identifier
		{
			get { return this.deviceName; }
		}

		#endregion

		private readonly string deviceName;
	}
}