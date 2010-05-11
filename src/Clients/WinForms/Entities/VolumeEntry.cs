using System;
using System.Collections.Generic;
using System.Linq;

namespace Gablarski.Clients.Windows.Entities
{
	public class VolumeEntry
	{
		public virtual int VolumeId
		{
			get; set;
		}

		public virtual int ServerId
		{
			get; set;
		}

		public virtual string Username
		{
			get; set;
		}

		public virtual float Gain
		{
			get; set;
		}
	}
}