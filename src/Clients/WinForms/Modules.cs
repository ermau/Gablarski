using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Gablarski.Audio;
using Gablarski.Clients.Input;
using Gablarski.Clients.Media;

namespace Gablarski.Clients.Windows
{
	public static class Modules
	{
		private static ModuleLoader<IInputProvider> inputLoader;
		public static IEnumerable<Type> Input
		{
			get
			{
				if (inputLoader == null)
					inputLoader = GetLoader<IInputProvider>();

				return inputLoader.GetImplementers();
			}
		}

		private static ModuleLoader<IPlaybackProvider> playback;
		public static IEnumerable<Type> Playback
		{
			get
			{
				if (playback == null)
					playback = GetLoader<IPlaybackProvider>();

				return playback.GetImplementers();
			}
		}

		private static ModuleLoader<ICaptureProvider> capture;
		public static IEnumerable<Type> Capture
		{
			get
			{
				if (capture == null)
					capture = GetLoader<ICaptureProvider>();

				return capture.GetImplementers();
			}
		}

		private static ModuleLoader<IAudioEngine> audioEngines;
		public static IEnumerable<Type> AudioEngine
		{
			get
			{
				if (audioEngines == null)
					audioEngines = GetLoader<IAudioEngine>();

				return audioEngines.GetImplementers();
			}
		}

		private static ModuleLoader<IMediaPlayer> mediaPlayers;
		public static IEnumerable<Type> MediaPlayers
		{
			get
			{
				if (mediaPlayers == null)
					mediaPlayers = GetLoader<IMediaPlayer>();

				return mediaPlayers.GetImplementers();
			}
		}

		private static ModuleLoader<INotifier> notifiers;
		public static IEnumerable<Type> Notifiers
		{
			get
			{
				if (notifiers == null)
					notifiers = GetLoader<INotifier> ();

				return notifiers.GetImplementers ();
			}
		}

		private static ModuleLoader<T> GetLoader<T> ()
			where T : class
		{
			return new ModuleLoader<T>(
						ModuleLoaderOptions.SearchAll,
						(new FileInfo (System.Diagnostics.Process.GetCurrentProcess ().MainModule.FileName)).Directory.FullName,
						Path.Combine (Environment.GetFolderPath (Environment.SpecialFolder.ApplicationData), "Gablarski"));
		}
	}
}