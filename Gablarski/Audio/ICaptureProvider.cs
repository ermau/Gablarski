using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gablarski.Audio
{
	public enum AudioFormat
	{
		Mono8Bit,
		Mono16Bit,
		Stereo8Bit,
		Stereo16Bit
	}

	public interface ICaptureProvider
		: IAudioDeviceProvider, IDisposable
	{
		event EventHandler<SamplesAvailableEventArgs> SamplesAvailable;

		/// <summary>
		/// Gets or sets the device to capture from.
		/// </summary>
		IAudioDevice Device { get; set; }

		/// <summary>
		/// Gets whether or not this provider is currently capturing audio.
		/// </summary>
		bool IsCapturing { get; }

		/// <summary>
		/// Gets whether or not this provider can capture stereo.
		/// </summary>
		bool CanCaptureStereo { get; }

		/// <summary>
		/// Number of samples available to be read.
		/// </summary>
		int AvailableSampleCount { get; }

		/// <summary>
		/// Begins a capture.
		/// </summary>
		/// <exception cref="InvalidOperationException">If <see cref="Device"/> is null.</exception>
		void BeginCapture (AudioFormat format);

		/// <summary>
		/// Ends a capture.
		/// </summary>
		/// <exception cref="InvalidOperationException">If <see cref="Device"/> is null.</exception>
		void EndCapture ();

		/// <summary>
		/// Reads all available samples from the provider.
		/// </summary>
		/// <returns>The samples</returns>
		/// <exception cref="InvalidOperationException">If <see cref="Device"/> is null.</exception>
		byte[] ReadSamples ();

		/// <exception cref="InvalidOperationException">If <see cref="Device"/> is null.</exception>
		byte[] ReadSamples (int samples);
	}

	public class SamplesAvailableEventArgs
		: EventArgs
	{
		public SamplesAvailableEventArgs (int samples)
		{
			this.Samples = samples;
		}

		public int Samples
		{
			get;
			private set;
		}
	}
}