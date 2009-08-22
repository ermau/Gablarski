using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gablarski.Client;

namespace Gablarski.Audio
{
	public enum AudioEngineCaptureMode
	{
		Explicit = 0,
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

	public class AudioEnginePlaybackOptions
	{
		
	}

	public interface IAudioEngine
	{
		/// <summary>
		/// Gets or sets the audio receiver.
		/// </summary>
		IAudioReceiver AudioReceiver { get; set; }

		/// <summary>
		/// Attaches a playback provider to all <paramref name="sources"/> not already attached, skipping any ClientAudioSources.
		/// </summary>
		void Attach (IPlaybackProvider playback, IEnumerable<AudioSource> sources, AudioEnginePlaybackOptions options);

		/// <summary>
		/// Attaches a playback provider to be used for the given source.
		/// </summary>
		void Attach (IPlaybackProvider playback, AudioSource source, AudioEnginePlaybackOptions options);

		/// <summary>
		/// Starts a capture with the given <paramref name="capture"/> pumped to the <paramref name="source"/> with the given <paramref name="options"/>.
		/// </summary>
		/// <param name="capture">The provider to pump the audio from. (If the device is not preselected, the default device will be used.)</param>
		/// <param name="source">The audio source to pump the audio to.</param>
		/// <param name="options">Capturing options.</param>
		void Attach (ICaptureProvider capture, AudioFormat format, ClientAudioSource source, AudioEngineCaptureOptions options);

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

		/// <summary>
		/// Starts the audio engine.
		/// </summary>
		void Start();

		/// <summary>
		/// Stops the audio engine and clears all attachments.
		/// </summary>
		void Stop();

		void BeginCapture (AudioSource source, ChannelInfo channel);
		void EndCapture (AudioSource source);
	}
}
