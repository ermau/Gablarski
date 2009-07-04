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
		/// <summary>
		/// Fired when a source finishes playing.
		/// </summary>
		event EventHandler<SourceFinishedEventArgs> SourceFinished;

		/// <summary>
		/// Gets or sets the playback device.
		/// </summary>
		IDevice Device { get; set; }

		/// <summary>
		/// Queues fixed-point PCM <paramref name="data"/> to be played back, owned by <paramref name="source"/>.
		/// </summary>
		/// <param name="source">The <see cref="AudioSource"/> the audio came from.</param>
		/// <param name="data">Fixed-point PCM data.</param>
		void QueuePlayback (AudioSource source, byte[] data);
	}

	public class SourceFinishedEventArgs
		: EventArgs
	{
		/// <summary>
		/// Initializes a new instance of <see cref="SourceFinishedEventArgs"/>
		/// </summary>
		/// <param name="source"></param>
		public SourceFinishedEventArgs (AudioSource source)
		{
			if (source == null)
				throw new ArgumentNullException("source");

			this.Source = source;
		}

		/// <summary>
		/// Gets the source that finished playing.
		/// </summary>
		public AudioSource Source
		{
			get; private set;
		}
	}
}