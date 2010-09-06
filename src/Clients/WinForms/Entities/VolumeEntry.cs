using System;
using System.Collections.Generic;
using System.Linq;

namespace Gablarski.Clients.Windows.Entities
{
	public class VolumeEntry
	{
		public VolumeEntry ()
			: this (0)
		{
			Gain = 1.0f;
		}

		public VolumeEntry (int id)
		{
			VolumeId = id;
		}

		public int VolumeId
		{
			get;
			private set;
		}

		public int ServerId
		{
			get;
			set;
		}

		public string Username
		{
			get;
			set;
		}

		public float Gain
		{
			get;
			set;
		}
	}
}