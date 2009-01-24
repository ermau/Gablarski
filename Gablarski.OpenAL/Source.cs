using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace Gablarski.OpenAL
{
	public class Source
		: IDisposable
	{
		internal Source (uint sourceID)
		{
			this.sourceID = sourceID;
		}

		public void Delete ()
		{
			uint[] id = new uint[] { this.sourceID };
			alDeleteSources (1, ref id);
			OpenAL.ErrorCheck ();
		}

		private readonly uint sourceID;

		#region IDisposable Members

		public void Dispose ()
		{
			Dispose (true);
		}

		private bool disposed;

		protected void Dispose (bool disposing)
		{
			if (this.disposed)
				return;

			if (!disposing)
				GC.SuppressFinalize (this);

			this.Delete ();
			this.disposed = true;
		}

		~Source ()
		{
			Dispose (false);
		}

		#endregion
		
		[DllImport ("OpenAL32.dll")]
		private static extern void alGenSources (int count, ref uint[] sources);

		[DllImport ("OpenAL32.dll")]
		private static extern void alDeleteSources (int count, ref uint[] sources);

		internal static int AvailableSources
		{
			get { return 16; }
		}

		public static Source[] GenerateSources (int count)
		{
			if (count > AvailableSources)
				return null;

			Source[] sources = new Source[count];

			uint[] sourceIDs = new uint[count];
			alGenSources (count, ref sourceIDs);
			OpenAL.ErrorCheck ();

			for (int i = 0; i < count; ++i)
				sources[i] = new Source (sourceIDs[i]);

			return sources;
		}
	}
}