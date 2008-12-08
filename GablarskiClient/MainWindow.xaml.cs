using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Gablarski.Client;
using Gablarski.Client.Providers.OpenAL;
using System.Windows.Threading;
using System.Threading;
using Tao.OpenAl;
using System.Diagnostics;
using Gablarski.Server;
using Gablarski.Server.Providers;
using Gablarski;

namespace GablarskiClient
{
	/// <summary>
	/// Interaction logic for Window1.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow ()
		{
			InitializeComponent ();
		}

		private Gablarski.Client.GablarskiClient client;
		private ICaptureProvider capture;

		private void connectButton_Click(object sender, RoutedEventArgs e)
		{
			client = new Gablarski.Client.GablarskiClient();
			client.Connected	+= client_Connected;
			client.Disconnected += client_Disconnected;
			client.LoggedIn		+= client_LoggedIn;
			client.UserLogin	+= client_UserLogin;
			client.UserLogout	+= client_UserLogout;
			client.VoiceReceived += client_VoiceReceived;
			client.Connect (this.host.Text, 6112);
			client.Login (this.nickname.Text);

			this.disconnectButton.IsEnabled = true;
			this.talk.IsEnabled = true;

			this.capture = new OpenALCaptureProvider ();
			this.capture.SetDevice (this.capture.GetDevices ().First ());
		}

		private OpenALPlaybackProvider playback;
		void client_VoiceReceived (object sender, Gablarski.VoiceEventArgs e)
		{
			this.Log ("Received voice data from: " + e.User.Username);

			if (this.playback == null)
				this.playback = new OpenALPlaybackProvider ();

			this.playback.QueuePlayback (e.VoiceData);
		}

		void client_UserLogout (object sender, UserEventArgs e)
		{
			this.Log (e.User.Username + " has disconnected.");
		}

		void client_UserLogin (object sender, UserEventArgs e)
		{
			this.Log (e.User.Username + " has connected.");
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

		private void Log (string log)
		{
			this.Dispatcher.BeginInvoke ((Action)delegate
			{
				this.log.Text += log + Environment.NewLine;
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
			public ServerLogListener (MainWindow window)
			{
				this.window = window;
			}

			private readonly MainWindow window;

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
				if (this.capture.ReadSamples (out samples))
					this.client.SendVoiceData (samples);
			}
		}
	}
}