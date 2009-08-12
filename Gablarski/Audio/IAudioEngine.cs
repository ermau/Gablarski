using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gablarski.Audio
{
	public enum AudioEngineCaptureMode
	{
		Signal = 0,
		Activated
	}

	public class AudioEngineCaptureOptions
	{
		public AudioEngineCaptureMode Mode
		{
			get; set;
		}

		public int VoiceActivityStartProbability
		{
			get; set;
		}
	}

	public interface IAudioEngine
	{
		/// <summary>
		/// Starts a capture with the given <paramref name="provider"/> pumped to the <paramref name="source"/> with the given <paramref name="options"/>.
		/// </summary>
		/// <param name="provider">The provider to pump the audio from. (If the device is not preselected, the default device will be used.)</param>
		/// <param name="source">The audio source to pump the audio to.</param>
		/// <param name="options">Capturing options.</param>
		void Attach (ICaptureProvider provider, AudioSource source, AudioEngineCaptureOptions options);

		/// <summary>
		/// Stops any captures on the given provider.
		/// </summary>
		/// <param name="provider">The provider to stop any captures for.</param>
		/// <returns><c>true</c> if there were any captures for the <paramref name="provider"/>.</returns>
		bool Detach (ICaptureProvider provider);

		/// <summary>
		/// Stops any capturing to <paramref name="source"/>.
		/// </summary>
		/// <param name="source">The source to stop any capturing for.</param>
		/// <returns><c>true</c> if any capturing was occuring for <paramref name="source"/>, <c>false</c> otherwise.</returns>
		bool Detach (AudioSource source);

		void BeginCapture (AudioSource source);
		void EndCapture (AudioSource source);
	}
}
