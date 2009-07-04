using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gablarski.Media.Sources;

namespace Gablarski.Client
{
	public interface IPlaybackProvider
		: IDeviceProvider, IDisposable
	{
		event EventHandler<SourceFinishedEventArgs> SourceFinished;

		IDevice Device { get; set; }

		void QueuePlayback (AudioSource source, byte[] data, int frequency);
	}

	public class SourceFinishedEventArgs
		: EventArgs
	{
		public SourceFinishedEventArgs (AudioSource source)
		{
			this.Source = source;
		}

		public AudioSource Source
		{
			get; private set;
		}
	}
}