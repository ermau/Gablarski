using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tao.OpenAl;
using System.Threading;

namespace Gablarski.Client.Providers.OpenAL
{
	internal unsafe class OpenALSourcePool
        : IDisposable
	{
		static public int TotalSourcesAvailable
		{
			get { return 16; }
		}

		public OpenALSourcePool (IntPtr device)
		{
			if (device == IntPtr.Zero)
				throw new ArgumentException ("device");

			this.device = device;

			this.collecting = true;
			(this.collectorThread = new Thread (this.Collector)
			                        {
			                        	IsBackground = true,
			                        	Name = "OpenAL Source Pool Collector"
			                        }).Start();
		}

		public int RequestSource (uint ownerID, bool stereo)
		{
			rwl.EnterUpgradeableReadLock ();

			int free = -1;
			foreach (var kvp in owners)
			{
				if (kvp.Value == 0)
				{
					free = kvp.Key;
					break;
				}
				else if (kvp.Value == ownerID)
				{
					rwl.ExitUpgradeableReadLock ();
					return kvp.Key;
				}
			}

			if (free == -1)
			{
				if (this.sourcesAvailable > 0)
					Al.alGenSources (1, out free);
				else
				{
					rwl.ExitUpgradeableReadLock();
					return -1;
				}
			}

			rwl.EnterWriteLock ();
			sourcesAvailable -= (stereo) ? 2 : 1;
			owners[free] = ownerID;
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

		private int sourcesAvailable = 16;
		private readonly Thread collectorThread;
		private bool collecting;
		private readonly ReaderWriterLockSlim rwl = new ReaderWriterLockSlim ();

		private readonly Dictionary<int, uint> owners = new Dictionary<int, uint> (TotalSourcesAvailable);

		private readonly IntPtr device;

		private void Collector ()
		{
			while (this.collecting)
			{
				List<int> toFree = new List<int> (TotalSourcesAvailable);

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
						FreeSource (sourceID);
				}
				rwl.ExitReadLock();

				Thread.Sleep (100);
			}
		}

		#region IDisposable Members

		public void Dispose ()
		{
			this.collecting = false;

			rwl.EnterWriteLock();
			owners.Clear ();

			//Al.alDeleteSources (this.sources.Length, this.sources);
			foreach (int source in owners.Keys)
				Al.alDeleteSources (1, (int*)source);

			rwl.ExitWriteLock();

			if (this.collectorThread != null)
				this.collectorThread.Join();
		}

		#endregion
	}
}