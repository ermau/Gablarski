using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace Gablarski.OpenAL
{
	public class SourceBuffer
	{
		internal SourceBuffer (uint bufferID)
		{
			this.bufferID = bufferID;
		}

		private readonly uint bufferID;

		public static SourceBuffer[] GenerateBuffers (int count)
		{
			SourceBuffer[] buffers = new SourceBuffer[count];

			uint[] bufferIDs = new uint[count];
			alGenBuffers (count, ref bufferIDs);
			OpenAL.ErrorCheck ();

			for (int i = 0; i < count; ++i)
				buffers[i] = new SourceBuffer (bufferIDs[i]);

			return buffers;
		}

		[DllImport ("OpenAL32.dll")]
		private static extern void alGenBuffers (int count, ref uint[] bufferIDs);
	}
}