using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tao.OpenAl;
using System.Threading;

namespace Gablarski.Client.Providers.OpenAL
{
	internal class OpenALSourcePool
        : IDisposable
	{
		static public int SourcesAvailable
		{
			get { return 16; }
		}

		public OpenALSourcePool (IntPtr device)
		{
			if (device == IntPtr.Zero)
				throw new ArgumentException ("device");

			this.device = device;

			// This assumes MONO.
			Al.alGenSources (SourcesAvailable, this.sources);

			for (int i = 0; i < this.sources.Length; ++i)
				owners[this.sources[i]] = 0;

			this.collecting = true;
			(this.collectorThread = new Thread (this.Collector)
			                        {
			                        	IsBackground = true,
			                        	Name = "OpenAL Source Pool Collector"
			                        }).Start();
		}

		public int RequestSource (uint playerID)
		{
			rwl.EnterUpgradeableReadLock ();
			int free = -1;
			foreach (var kvp in owners)
			{
				if (free == -1 && kvp.Value == 0)
					free = kvp.Key;
				else if (kvp.Value == playerID)
				{
					rwl.ExitUpgradeableReadLock ();
					return kvp.Key;
				}
			}

			if (free == -1)
			{
				rwl.EnterUpgradeableReadLock();
				return -1;
			}

			rwl.EnterWriteLock ();
			owners[free] = playerID;
			rwl.ExitWriteLock ();
			rwl.ExitUpgradeableReadLock ();

			return free;
		}

		public void FreeSource (int source)
		{
			rwl.EnterWriteLock ();
			owners[source] = 0;
			rwl.ExitWriteLock ();
		}

		private readonly Thread collectorThread;
		private bool collecting;
		private readonly ReaderWriterLockSlim rwl = new ReaderWriterLockSlim ();

		// ReSharper disable FieldCanBeMadeReadOnly
		private int[] sources = new int[SourcesAvailable];
		// ReSharper restore FieldCanBeMadeReadOnly

		private readonly Dictionary<int, uint> owners = new Dictionary<int, uint> (SourcesAvailable);

		private readonly IntPtr device;

		private void Collector ()
		{
			while (this.collecting)
			{
				List<int> toFree = new List<int>(this.sources.Length);

				rwl.EnterReadLock();
				foreach (int sourceID in owners.Keys)
				{
					int buffers;
					Al.alGetSourcei (sourceID, Al.AL_BUFFERS_PROCESSED, out buffers);
					if (buffers > 0)
					{
						Al.alSourceUnqueueBuffers (sourceID, buffers, ref buffers);
						Al.alDeleteBuffers (buffers, ref buffers);
					}

					int state;
					Al.alGetSourcei (sourceID, Al.AL_SOURCE_STATE, out state);
					if (state != Al.AL_PLAYING)
						toFree.Add (sourceID);
				}
				rwl.ExitReadLock();

				toFree.ForEach (this.FreeSource);

				Thread.Sleep (100);
			}
		}

		#region IDisposable Members

		public void Dispose ()
		{
			this.collecting = false;

			rwl.EnterWriteLock();
			owners.Clear ();

			Al.alDeleteSources (this.sources.Length, this.sources);

			rwl.ExitWriteLock();

			if (this.collectorThread != null)
				this.collectorThread.Join();
		}

		#endregion
	}
}