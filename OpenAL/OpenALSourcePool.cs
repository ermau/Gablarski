using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tao.OpenAl;

namespace Gablarski.Client.Providers.OpenAL
{
	internal unsafe class OpenALSourcePool
	{
		public OpenALSourcePool (IntPtr device, IntPtr context)
		{
			if (device == IntPtr.Zero)
				throw new ArgumentException ("device");

			if (context == IntPtr.Zero)
				throw new ArgumentException ("context");

			this.device = device;
			this.context = context;

			Al.alGenSources (16, this.sources);

			for (int i = 0; i < this.sources.Length; ++i)
				owners[this.sources[i]] = 0;
		}

		private int[] sources = new int[16];
		private Dictionary<int, uint> owners = new Dictionary<int, uint> ();

		private readonly IntPtr device;
		private readonly IntPtr context;
	}
}