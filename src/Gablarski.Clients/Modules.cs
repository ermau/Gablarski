using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Linq;
using Gablarski.Audio;
using Gablarski.Clients.Input;
using Gablarski.Clients.Media;

namespace Gablarski.Clients.Windows
{
	public class Modules
	{
		private Modules()
		{
			new CompositionContainer(new DirectoryCatalog (".")).ComposeParts (this);
		}

		public static void Reload()
		{
			modules.Recompose();
		}

		public static IEnumerable<IInputProvider> Input
		{
			get { return modules.input; }
		}

		public static IEnumerable<IAudioPlaybackProvider> Playback
		{
			get { return modules.playback; }
		}

		public static IEnumerable<IAudioCaptureProvider> Capture
		{
			get { return modules.capture; }
		}

		public static IEnumerable<IAudioEngine> AudioEngines
		{
			get { return modules.audioEngines; }
		}

		public static IEnumerable<IMediaPlayer> MediaPlayers
		{
			get { return modules.mediaPlayers; }
		}

		public static IEnumerable<INotifier> Notifiers
		{
			get { return modules.notifiers; }
		}

		public static IEnumerable<ITextToSpeech> TextToSpeech
		{
			get { return modules.tts; }
		}

		private static readonly Modules modules = new Modules();

		[ImportMany (AllowRecomposition = true)]
		private IEnumerable<ITextToSpeech> tts
		{
			get;
			set;
		}

		[ImportMany (AllowRecomposition = true)]
		private IEnumerable<IInputProvider> input
		{
			get;
			set;
		}

		[ImportMany (AllowRecomposition = true)]
		private IEnumerable<IAudioEngine> audioEngines
		{
			get;
			set;
		}

		[ImportMany (AllowRecomposition = true)]
		private IEnumerable<INotifier> notifiers
		{
			get;
			set;
		}

		[ImportMany (AllowRecomposition = true)]
		private IEnumerable<IMediaPlayer> mediaPlayers
		{
			get;
			set;
		}

		[ImportMany (AllowRecomposition = true)]
		private IEnumerable<IAudioCaptureProvider> capture
		{
			get;
			set;
		}

		[ImportMany (AllowRecomposition = true)]
		private IEnumerable<IAudioPlaybackProvider> playback
		{
			get;
			set;
		}

		private void Recompose()
		{
		}
	}
}