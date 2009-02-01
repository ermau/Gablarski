using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
//using FFmpegSharp.Interop;
//using FFmpegSharp.Interop.Codec;
//using FFmpegSharp.Interop.Format;
using Gablarski.Client.Providers;
using Gablarski.Server;
using Gablarski.Server.Providers;

namespace Gablarski.Client
{
	/// <summary>
	/// Interaction logic for Window1.xaml
	/// </summary>
	public partial class DiagWindow : Window
	{
		public DiagWindow ()
		{
			InitializeComponent ();
			PushToTalk.Talking += PushToTalk_Talking;
			PushToTalk.StoppedTalking += PushToTalk_StoppedTalking;
		}

		void PushToTalk_StoppedTalking (object sender, EventArgs e)
		{
			if (!client.IsLoggedin)
				return;

			this.talkStatus.Source = new BitmapImage (new Uri ("./resources/sound_none.png", UriKind.Relative));
			capture.EndCapture();
		}

		void PushToTalk_Talking (object sender, EventArgs e)
		{
			if (!client.IsLoggedin)
				return;

			this.talkStatus.Source = new BitmapImage (new Uri ("./resources/sound.png", UriKind.Relative));
			capture.StartCapture();
		}

		private GablarskiClient client;
		private ICaptureProvider capture;

		private void connectButton_Click(object sender, RoutedEventArgs e)
		{
			client = new Gablarski.Client.GablarskiClient();
			client.Connected	+= client_Connected;
			client.Disconnected += client_Disconnected;
			client.LoggedIn		+= client_LoggedIn;
			client.UserLogin	+= client_UserLogin;
			client.UserLogout	+= client_UserLogout;
			client.AudioReceived+= client_AudioReceived;
			client.Connect (this.host.Text, 6112);
			client.Login (this.nickname.Text);

			this.disconnectButton.IsEnabled = true;
			this.talk.IsEnabled = true;

			this.capture = new OpenALCaptureProvider ();
			this.capture.CaptureDevice = this.capture.GetDevices ().First ();
			this.capture.SamplesAvailable += capture_SamplesAvailable;
		}

		void capture_SamplesAvailable (object sender, SamplesEventArgs e)
		{
			this.client.SendMedia (this.client.VoiceSource, e.Samples);
		}

		private OpenALPlaybackProvider playback = new OpenALPlaybackProvider ();
		void client_AudioReceived (object sender, AudioEventArgs e)
		{
			this.Log ("Received audio data from source: " + e.Source.ID);

			if (this.playback == null)
				this.playback = new OpenALPlaybackProvider ();

			this.playback.QueuePlayback (e.Source.Codec.Decode (e.Data), e.Source);
		}

		void client_UserLogout (object sender, UserEventArgs e)
		{
			this.Log (e.User.Nickname + " has disconnected.");
		}

		void client_UserLogin (object sender, UserEventArgs e)
		{
			this.Log (e.User.Nickname + " has connected.");
		}

		void client_LoggedIn (object sender, ConnectionEventArgs e)
		{
			this.SetStatusImage ("./Resources/key.png");
			this.Log ("Logged in.");
		}

		private void client_Disconnected (object sender, ReasonEventArgs e)
		{
			this.SetStatusImage ("./Resources/disconnect.png");
			this.Log ("Disconnected: " + e.Reason);

			this.disconnectButton.IsEnabled = false;
			this.talk.IsEnabled = false;
		}

		private void client_Connected (object sender, ConnectionEventArgs e)
		{
			this.SetStatusImage ("./Resources/connect.png");
			this.Log ("Connected.");
		}

		private void SetStatusImage (string uri)
		{
			this.Dispatcher.BeginInvoke((Action)delegate
			{
				this.statusImage.Source =
					new BitmapImage(new Uri(uri, UriKind.Relative));
			});
		}

		private void Log (string logentry)
		{
			this.Dispatcher.BeginInvoke ((Action)delegate
			{
				this.log.Text += logentry + Environment.NewLine;
			});
		}

		private void Window_Closed (object sender, EventArgs e)
		{
			this.client.Disconnect ();
			this.Server.Shutdown();
		}

		private void disconnectButton_Click (object sender, RoutedEventArgs e)
		{
			this.client.Disconnect ();
		}

		private static IAuthProvider Authentication;
		private GablarskiServer Server;
		private void startServer_Click (object sender, RoutedEventArgs e)
		{
			this.startServer.IsEnabled = false;
			this.stopServer.IsEnabled = true;

			Trace.UseGlobalLock = true;
			Trace.Listeners.Add (new ServerLogListener (this));

			Authentication = new NicknameAuthenticationProvider ();

			Server = new GablarskiServer (Authentication);
			Server.ClientConnected += (s, ea) => Trace.WriteLine ("Client connected.");
			Server.Start ();
		}

		private class ServerLogListener
			: TraceListener
		{
			public ServerLogListener (DiagWindow window)
			{
				this.window = window;
			}

			private readonly DiagWindow window;

			public override void Write (string message)
			{
				this.window.Dispatcher.BeginInvoke ((Action)delegate
				{
					this.window.serverLog.Text += message;
				});
			}

			public override void WriteLine (string message)
			{
				this.window.Dispatcher.BeginInvoke ((Action)delegate
				{
					this.window.serverLog.Text += message + Environment.NewLine;
				});
			}
		}

		private bool recording = false;
		private void talk_Click (object sender, RoutedEventArgs e)
		{
			if (!recording)
			{
				this.talkStatus.Source = new BitmapImage (new Uri ("./resources/sound.png", UriKind.Relative));
				this.capture.StartCapture ();
				recording = true;
			}
			else
			{
				this.talkStatus.Source = new BitmapImage (new Uri ("./resources/sound_none.png", UriKind.Relative));
				recording = false;

				byte[] samples;

				this.capture.EndCapture ();
			}
		}

		private void pushtotalk_PreviewKeyUp (object sender, KeyEventArgs e)
		{
			try
			{
				PushToTalk.Keys = (System.Windows.Forms.Keys)Enum.Parse (typeof (System.Windows.Forms.Keys), e.Key.ToString (), true);
				this.pushtotalk.Text = PushToTalk.Keys.ToString();
				e.Handled = true;
			}
			catch
			{}
		}

		//class TestSource
		//: IMediaSource
		//{
		//    #region IMediaSource Members

		//    public int ID
		//    {
		//        get { return 0; }
		//    }

		//    public MediaSourceType Type
		//    {
		//        get { return MediaSourceType.Music; }
		//    }

		//    public IMediaCodec Codec
		//    {
		//        get { return null; }
		//    }

		//    public IMediaCodec VideoCodec
		//    {
		//        get { return null; }
		//    }

		//    public IUser Owner
		//    {
		//        get { return null; }
		//    }

		//    #endregion
		//}
		
		//private void button1_Click (object sender, RoutedEventArgs e)
		//{
			//OpenALPlaybackProvider provider = new OpenALPlaybackProvider();

			//FFmpeg.av_register_all();

			//AVFormatContext fcontext;
			//if (FFmpeg.av_open_input_file(out fcontext, @"D:\Music\Weird Al Yankovic\Straight Outta Lynwood\01 White And Nerdy.mp3") != AVError.OK)
			//{
			//    Console.WriteLine("input file error");
			//    Environment.Exit(1);
			//}

			//int streamIndex = 0;
			//AVStream* audio;
			//AVCodecContext ccontext = default(AVCodecContext);
			//for (int i = 0; i < fcontext.streams.Length; ++i)
			//{
			//    if (fcontext.streams[i]->codec->codec_type != CodecType.CODEC_TYPE_AUDIO)
			//        continue;

			//    streamIndex = i;
			//    audio = fcontext.streams[i];
			//    ccontext = *audio->codec;
			//    break;
			//}

			//AVCodec* codec = FFmpeg.avcodec_find_decoder(ccontext.codec_id);
			//if (codec == null)
			//{
			//    Console.WriteLine("codec error");
			//    Environment.Exit(1);
			//}

			//var t = new TestSource();

			//FFmpeg.avcodec_open(ref ccontext, codec);

			//AVFrame* frame = FFmpeg.avcodec_alloc_frame();

			//int frame_size_ptr = ccontext.frame_size;
			//AVPacket packet = default(AVPacket);
			//while (FFmpeg.av_read_frame(ref fcontext, ref packet) >= 0)
			//{
			//    if (packet.stream_index != streamIndex)
			//        continue;

			//    byte[] data = new byte[packet.size];
			//    Marshal.Copy(packet.data, data, 0, packet.size);
			//    short[] samples = new short[ccontext.frame_size];
			//    FFmpeg.avcodec_decode_audio2(ref ccontext, samples, ref frame_size_ptr, data, packet.size);

			//    byte[] buffer = new byte[samples.Length * 2];
			//    for (int i = 0; i < samples.Length; i += 2)
			//    {
			//        buffer[i] = (byte)samples[i];
			//        buffer[i + 1] = (byte)(samples[i] >> 8);
			//    }

			//    provider.QueuePlayback(buffer, t);
			//}
		//}
	}
}