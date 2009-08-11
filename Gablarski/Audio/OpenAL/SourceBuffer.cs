using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Security;

namespace Gablarski.Audio.OpenAL
{
	[SuppressUnmanagedCodeSecurity]
	public class SourceBuffer
		: IDisposable
	{
		internal SourceBuffer (uint bufferID)
		{
			this.bufferID = bufferID;
		}

		public void Buffer (byte[] data, AudioFormat format, uint frequency)
		{
			alBufferData (this.bufferID, format, data, data.Length, frequency);
			Audio.OpenAL.OpenAL.ErrorCheck ();
		}

		#region IDisposable Members
		private bool disposed;

		public void Dispose ()
		{
			this.Dispose (true);
			GC.SuppressFinalize (this);
		}

		protected virtual void Dispose (bool disposing)
		{
			if (this.disposed)
				return;

			alDeleteBuffers (1, new[] { this.bufferID });
			Audio.OpenAL.OpenAL.ErrorCheck ();

			lock (lck)
			{
				Buffers.Remove (this.bufferID);
			}

			this.disposed = true;
		}

		#endregion

		internal readonly uint bufferID;

		public static SourceBuffer Generate ()
		{
			return Generate (1)[0];
		}

		public static SourceBuffer[] Generate (int count)
		{
			lock (lck)
			{
				if (Buffers == null)
					Buffers = new Dictionary<uint, SourceBuffer> (count);

				SourceBuffer[] buffers = new SourceBuffer[count];

				uint[] bufferIDs = new uint[count];
				alGenBuffers (count, bufferIDs);
				Audio.OpenAL.OpenAL.ErrorCheck ();

				for (int i = 0; i < count; ++i)
				{
					buffers[i] = new SourceBuffer (bufferIDs[i]);
					Buffers.Add (buffers[i].bufferID, buffers[i]);
				}

				return buffers;
			}
		}

		//public static void Delete (this IEnumerable<SourceBuffer> self)
		//{
		//    uint[] bufferIDs = self.Select (b => b.bufferID).ToArray ();
		//    alDeleteBuffers (bufferIDs.Length, bufferIDs);
		//    OpenAL.ErrorCheck ();
		//}

		#region Imports
		[DllImport ("OpenAL32.dll", CallingConvention = CallingConvention.Cdecl)]
		private static extern void alGenBuffers (int count, uint[] bufferIDs);

		[DllImport ("OpenAL32.dll", CallingConvention = CallingConvention.Cdecl)]
		private static extern void alBufferData (uint bufferID, AudioFormat format, byte[] data, int byteSize, uint frequency);

		[DllImport ("OpenAL32.dll", CallingConvention = CallingConvention.Cdecl)]
		private static extern void alDeleteBuffers (int numBuffers, uint[] bufferIDs);
		#endregion

		private static object lck = new object ();

		private static Dictionary<uint, SourceBuffer> Buffers
		{
			get;
			set;
		}

		internal static SourceBuffer GetBuffer (uint bufferID)
		{
			lock (lck)
			{
				return Buffers[bufferID];
			}
		}
	}
}